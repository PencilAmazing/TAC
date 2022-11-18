using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using TAC.Editor;
using TAC.Logic;
using TAC.Render;
using static System.Math;

namespace TAC.World
{
	public class Scene
	{
		public List<Unit> units;
		public List<ParticleEffect> particleEffects;

		public Floor floor;

		public ResourceCache cache;
		public Renderer renderer { get; }
		public Position size { get; }
		public bool isEdit;

		private Stack<DebugText> debugStack;
		//public List<Position> debugPath;

		private Action currentAction;

		public Scene(Position size, Renderer renderer, ResourceCache cache, bool isEdit)
		{
			this.size = size;
			this.cache = cache;
			this.renderer = renderer;
			this.units = new();
			particleEffects = new();
			this.isEdit = isEdit;
			currentAction = null;

			floor = new Floor(size.x, size.y);
			floor.CreateTexture(cache);

			debugStack = new Stack<DebugText>();
		}

		public virtual void Think(float deltaTime)
		{
			// Is this necessary? Make it opt in if anything
			//foreach (Unit unit in units)
			//unit.Think(deltaTime);

			if (currentAction != null) {
				currentAction.Think(deltaTime);
				if (currentAction.isDone) ClearCurrentAction();
			}

			// Copy list
			foreach (ParticleEffect effect in particleEffects.ToArray()) {
				if (effect.phase < 0) particleEffects.Remove(effect);
			}
		}

		public virtual void Draw(Camera3D camera)
		{
			renderer.DrawSkybox(camera, cache);
			renderer.DrawFloor(camera, floor, cache);

			// Draw wall
			for (int i = 0; i < floor.length; i++) {
				for (int j = 0; j < floor.width; j++) {
					Tile tile = floor.GetTile(i, j);
					if (tile.North > 0)
						renderer.DrawWall(camera, new Vector3(i, 0, j), false, tile.HasWall(Wall.FlipNorth), cache.brushes[tile.North], cache);
					if (tile.West > 0)
						renderer.DrawWall(camera, new Vector3(i, 0, j), true, tile.HasWall(Wall.FlipWest), cache.brushes[tile.West], cache);
				}
			}

			renderer.DrawUnits(camera, units, cache);
			foreach (ParticleEffect effect in particleEffects) {
				effect.phase += 1;
				renderer.DrawEffect(camera, effect, cache);
			}
			renderer.DrawUnitDebug(camera, units, cache);
		}

		public void PushDebugText(DebugText text) => debugStack.Push(text);

		public void DrawDebug()
		{
			while (debugStack.Count > 0) {
				DebugText text = debugStack.Pop();
				Raylib.DrawText(text.text, text.posx, text.posy, text.fontSize, text.color);
			}
			debugStack.TrimExcess();
		}

		public virtual void DrawDebug3D(Camera3D camera)
		{
			//if (debugPath != null) renderer.DrawDebugPath(debugPath.ToArray());

			ActionMoveUnit move = GetCurrentAction() as ActionMoveUnit;
			if (move != null) {
				renderer.DrawDebugPath(move.path.path.ToArray());
				return;
			}
			ActionSelectTarget select = GetCurrentAction() as ActionSelectTarget;
			if (select != null) {
				renderer.DrawDebugPath(select.line);
				if (select.collision.hit)
					renderer.DrawDebugLine(select.start.position.ToVector3() + select.start.equipOffset, select.collision.point, Color.BEIGE);
				else
					renderer.DrawDebugLine(select.start.position.ToVector3() + select.start.equipOffset, select.line[select.line.Length - 1].ToVector3(), Color.BEIGE);
				return;
			}
		}

		public Model GetFloorQuad() => floor.GetQuad();

		public Tile GetTile(Position pos) => floor.GetTile(pos.x, pos.z);

		/// <summary>
		/// Is position within size of scene?
		/// </summary>
		public bool IsTileWithinBounds(Position pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
				   pos.x < size.x && pos.y < size.y && pos.z < size.z;
		}

		public bool IsTileOccupied(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile != Tile.nullTile && (tile.unit != null); // Or tile has object
		}

		public void ToggleWall(Position pos, Wall wall)
		{
			int result = (floor[pos.x, pos.z].walls) ^ (byte)wall;
			floor[pos.x, pos.z].walls = (byte)(result);
		}

		/// <summary>
		/// Calls ToggleWall aswell
		/// </summary>
		public void ToggleBrush(Position pos, Wall wall, int brushID)
		{
			if (wall == Wall.North) {
				if (floor[pos.x, pos.z].North > 0)
					floor[pos.x, pos.z].North = 0;
				else
					floor[pos.x, pos.z].North = brushID;
				ToggleWall(pos, wall);
			} else if (wall == Wall.West) {
				if (floor[pos.x, pos.z].West > 0)
					floor[pos.x, pos.z].West = 0;
				else
					floor[pos.x, pos.z].West = brushID;
				ToggleWall(pos, wall);
			}
		}

		// https://www.redblobgames.com/grids/line-drawing.html#supercover
		// https://github.com/cgyurgyik/fast-voxel-traversal-algorithm/blob/master/overview/FastVoxelTraversalOverview.md
		// https://github.com/francisengelmann/fast_voxel_traversal/blob/master/main.cpp
		// FIXME Supercover line does not include initial tile in result
		public Position[] GetSupercoverLine(Position origin, Position p1)
		{
			Vector3 ray = ((p1 - origin).ToVector3());

			int stepX = Abs(ray.X) < float.Epsilon ? 0 : (ray.X < 0 ? -1 : 1);
			int stepY = Abs(ray.Y) < float.Epsilon ? 0 : (ray.Y < 0 ? -1 : 1);
			int stepZ = Abs(ray.Z) < float.Epsilon ? 0 : (ray.Z < 0 ? -1 : 1);

			Position i = origin;
			List<Position> points = new List<Position>() { origin };

			if (stepX == 0 && stepY == 0 && stepZ == 0) return points.ToArray();

			float tMaxX = stepX != 0 ? (i.x + stepX - origin.x) / ray.X : float.MaxValue;
			float tMaxY = stepY != 0 ? (i.y + stepY - origin.y) / ray.Y : float.MaxValue;
			float tMaxZ = stepZ != 0 ? (i.z + stepZ - origin.z) / ray.Z : float.MaxValue;

			float tDeltaX = stepX != 0 ? 1 / ray.X * stepX : float.MaxValue;
			float tDeltaY = stepY != 0 ? 1 / ray.Y * stepY : float.MaxValue;
			float tDeltaZ = stepZ != 0 ? 1 / ray.Z * stepZ : float.MaxValue;

			while (IsTileWithinBounds(i)) {
				if (tMaxX < tMaxY) {
					if (tMaxX < tMaxZ) {
						i.x += stepX;
						tMaxX += tDeltaX;
					} else {
						i.z += stepZ;
						tMaxZ += tDeltaZ;
					}
				} else {
					if (tMaxY < tMaxZ) {
						i.y += stepY;
						tMaxY += tDeltaY;
					} else {
						i.z += stepZ;
						tMaxZ += tDeltaZ;
					}
				}
				points.Add(i);
			}

			return points.ToArray();
		}

		/// <summary>
		/// True if adjacent direction is blocked
		/// </summary>
		/// <param name="dir">Direction to walk towards</param>
		public bool TestDirection(Position pos, UnitDirection dir)
		{
			// I really hope the compiler fixes this
			int x = pos.x;
			int z = pos.z;
			// Can't be arsed to inline
			// Walls around this tile
			bool north = floor.GetTile(x, z).North > 0;
			bool west = floor.GetTile(x, z).West > 0;
			bool south = floor.GetTile(x, z + 1).North > 0;
			bool east = floor.GetTile(x + 1, z).West > 0;

			if (dir == UnitDirection.North)
				return north;
			if (dir == UnitDirection.West)
				return west;
			if (dir == UnitDirection.South)
				return south;
			if (dir == UnitDirection.East)
				return east;

			if (dir == UnitDirection.NorthEast) {
				return north || east || floor.GetTile(x + 1, z - 1).West > 0 || floor.GetTile(x + 1, z).North > 0;
			} else if (dir == UnitDirection.NorthWest) {
				return north || west || floor.GetTile(x, z - 1).West > 0 || floor.GetTile(x - 1, z).North > 0;
			} else if (dir == UnitDirection.SouthEast) {
				return south || east || floor.GetTile(x + 1, z + 1).North > 0 || floor.GetTile(x + 1, z + 1).West > 0;
			} else if (dir == UnitDirection.SouthWest) {
				return south || west || floor.GetTile(x - 1, z + 1).North > 0 || floor.GetTile(x, z + 1).West > 0;
			} else return false;
		}

		public Action GetCurrentAction() => currentAction;
		public void SetCurrentAction(Action action) => currentAction = action;
		public void ClearCurrentAction() => currentAction = currentAction.NextAction();

		public void AddUnit(Unit unit)
		{
			if (unit == null)
				return;
			units.Add(unit);
			Tile tile = floor.GetTile(unit.position.x, unit.position.z);
			tile.unit = unit;
			floor.SetTile(unit.position, tile);
		}

		public void PushActionMoveUnit(Unit unit, Position goal)
		{
			if (unit == null || currentAction != null) return;
			Pathfinding path = new Pathfinding(this);
			if (path.FindPathForUnit(unit, goal)) {
				SetCurrentAction(new ActionMoveUnit(this, path));
			}
		}

		public void PushActionSelectTarget(Unit unit, Item item, Position target)
		{
			// Don't push action if unit is null or there's 
			// already an action in place
			if (unit == null || currentAction != null) return;
			if (IsTileWithinBounds(target)) {
				SetCurrentAction(new ActionSelectTarget(this, unit, item, target));
			}
		}

		public Unit GetUnit(Position pos)
		{
			return floor.GetTile(pos.x, pos.z).unit;
		}

		public void AddParticleEffect(ParticleEffect effect)
		{
			if (effect != null) particleEffects.Add(effect);
		}

		public void RemoveParticleEffect(ParticleEffect effect)
		{
			if (effect != null) particleEffects.Remove(effect);
		}
	}
}
