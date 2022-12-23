using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TAC.Editor;
using TAC.Logic;
using TAC.Render;
using static System.Math;
using static TAC.World.Position;

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

		// Map tile types to texture names
		// Refreshed each map load
		// Used to translate between references during disk IO
		public List<Texture> TileTypeMap { get; private set; }
		public int AddTileToMap(Texture tile)
		{
			TileTypeMap.Add(tile);
			return TileTypeMap.Count - 1;
		}
		public List<Brush> BrushTypeMap { get; private set; }
		public int AddBrushToMap(Brush brush)
		{
			BrushTypeMap.Add(brush);
			return BrushTypeMap.Count - 1;
		}

		// TODO: replace with action stack?
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

			debugStack = new Stack<DebugText>();

			TileTypeMap = new List<Texture>();
			BrushTypeMap = new List<Brush>();
		}

		~Scene()
		{
			TileTypeMap.Clear();
			BrushTypeMap.Clear();
		}

		public void AddFloor(Floor NewFloor)
		{
			this.floor = NewFloor;
			floor.CreateTexture(TileTypeMap);
		}

		public virtual void Think(float deltaTime)
		{
			// Is this necessary? Make it opt in if anything
			//foreach (Unit unit in units) {
			//	//unit.Think(deltaTime);
			//	if (!unit.IsAlive()) {
			//		units.Remove(unit);
			//	}
			//}

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
						renderer.DrawWall(camera, new Vector3(i, 0, j), false, tile.HasWall(Wall.FlipNorth), BrushTypeMap[tile.North - 1], cache);
					if (tile.West > 0)
						renderer.DrawWall(camera, new Vector3(i, 0, j), true, tile.HasWall(Wall.FlipWest), BrushTypeMap[tile.West - 1], cache);
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
			//Raylib.DrawSphere(Vector3.Zero, 0.1f, Color.PINK);

			ActionMoveUnit move = GetCurrentAction() as ActionMoveUnit;
			if (move != null) {
				renderer.DrawDebugPath(move.path.path.ToArray());
			}
			ActionSelectTarget select = GetCurrentAction() as ActionSelectTarget;
			if (select != null) {
				renderer.DrawDebugPath(select.line);
				if (select.collision.hit)
					renderer.DrawDebugLine(select.unit.position.ToVector3() + select.unit.equipOffset, select.collision.point, Color.BEIGE);
				else
					renderer.DrawDebugLine(select.unit.position.ToVector3() + select.unit.equipOffset, select.target.ToVector3(), Color.BEIGE);
			}

			foreach (Unit unit in units)
				Raylib.DrawBoundingBox(unit.GetUnitBoundingBox(), Color.ORANGE);
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

		/// <summary>
		/// Does tile contain a unit?
		/// </summary>
		public bool IsTileOccupied(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile != Tile.nullTile && (tile.unit != null); // Or tile has object
		}

		/// <summary>
		/// Does tile contain a unit or a thing?
		/// </summary>
		public bool IsTileImpassable(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile != Tile.nullTile && (tile.HasThing() || tile.HasUnit());
		}

		/// <summary>
		/// Does tile block line of sight or line of fire?
		/// </summary>
		public bool IsTileBlocking(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile != Tile.nullTile && (tile.HasWall(Wall.North | Wall.West) || tile.HasThing());
		}

		public void ToggleWall(Position pos, Wall wall)
		{
			// Do your magic, rosyln
			Wall select = (Wall)floor[pos.x, pos.z].walls;
			select ^= wall;
			// Clear flip bits if wall bits are not set
			select &= (Wall)((int)select << 2);
			floor[pos.x, pos.z].walls ^= (byte)select;
		}

		public void SetWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls = (byte)wall;
		}

		public void ClearWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls &= (byte)~wall;
		}

		public void ToggleBrush(Position pos, Wall wall, Brush brush)
		{
			int brushID = BrushTypeMap.IndexOf(brush) + 1;
			ToggleBrush(pos, wall, brushID);
		}

		/// <summary>
		/// Calls ToggleWall aswell
		/// </summary>
		public void ToggleBrush(Position pos, Wall wall, int brushID)
		{
			if (wall.HasFlag(Wall.North)) {
				if (floor[pos.x, pos.z].North > 0)
					floor[pos.x, pos.z].North = 0;
				else
					floor[pos.x, pos.z].North = brushID;
				ToggleWall(pos, wall);
			} else if (wall.HasFlag(Wall.West)) {
				if (floor[pos.x, pos.z].West > 0)
					floor[pos.x, pos.z].West = 0;
				else
					floor[pos.x, pos.z].West = brushID;
				ToggleWall(pos, wall);
			}
		}

		public void ClearBrush(Position pos, Wall wall)
		{
			if (wall == Wall.North) {
				floor[pos.x, pos.z].North = 0;
				ClearWall(pos, Wall.North | Wall.FlipNorth);
			} else if (wall == Wall.West) {
				floor[pos.x, pos.z].West = 0;
				ClearWall(pos, Wall.West | Wall.FlipWest);
			}
		}

		public Position[] GetSupercoverLine(Position origin, Position end) => GetSupercoverLine(origin.ToVector3(), end.ToVector3());

		// https://www.redblobgames.com/grids/line-drawing.html#supercover
		// https://github.com/cgyurgyik/fast-voxel-traversal-algorithm/blob/master/overview/FastVoxelTraversalOverview.md
		// https://gitlab.com/athilenius/fast-voxel-traversal-rs/-/blob/main/src/raycast_3d.rs
		// Supercover line does include initial tile in result because line technically passes through it
		public Position[] GetSupercoverLine(Vector3 origin, Vector3 end)
		{
			Vector3 d = Vector3.Normalize(end - origin);

			int stepX = Sign(d.X);
			int stepY = Sign(d.Y);
			int stepZ = Sign(d.Z);

			// Starting voxel
			Position i = new Position(origin);
			List<Position> points = new List<Position>() { new Position(origin) };

			if (Vector3.DistanceSquared(end, origin) < float.Epsilon) return points.ToArray();

			float tDeltaX = Abs(d.X) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.X);
			float tDeltaY = Abs(d.Y) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.Y);
			float tDeltaZ = Abs(d.Z) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.Z);

			// Offset by 0.5f to find fractional part, since out tiles are centered at 0,0
			// Other algorithms have tiles centered at 0.5, 0.5, so we try to cancel it out
			// Dont try to remove this \/, this is cell size and is 1.0f for now
			float distX = stepX > 0 ? (1.0f - 0.5f - origin.X + i.x) : (origin.X - i.x + 0.5f);
			float distY = stepY > 0 ? (1.0f - 0.5f - origin.Y + i.y) : (origin.Y - i.y + 0.5f);
			float distZ = stepZ > 0 ? (1.0f - 0.5f - origin.Z + i.z) : (origin.Z - i.z + 0.5f);

			float tMaxX = tDeltaX < float.PositiveInfinity ? distX * tDeltaX : float.PositiveInfinity;
			float tMaxY = tDeltaY < float.PositiveInfinity ? distY * tDeltaY : float.PositiveInfinity;
			float tMaxZ = tDeltaZ < float.PositiveInfinity ? distZ * tDeltaZ : float.PositiveInfinity;

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
			// TODO think of units and objects as well!
			// Somethings can block sight but not movement
			// I really hope the compiler fixes this
			int x = pos.x;
			int z = pos.z;

			// Blocking cardinal direction movement
			bool north = floor.GetTile(x, z).HasWall(Wall.North) || IsTileImpassable(pos - PositiveZ);
			bool west = floor.GetTile(x, z).HasWall(Wall.West) || IsTileImpassable(pos - PositiveX);
			bool south = floor.GetTile(x, z + 1).HasWall(Wall.North) || IsTileImpassable(pos + PositiveZ);
			bool east = floor.GetTile(x + 1, z).HasWall(Wall.West) || IsTileImpassable(pos + PositiveX);

			bool northeast = floor.GetTile(pos + PositiveX - PositiveZ).HasWall(Wall.West) ||
							 floor.GetTile(pos + PositiveX).HasWall(Wall.North) ||
							 IsTileImpassable(pos + PositiveX - PositiveZ);
			bool northwest = floor.GetTile(pos - PositiveZ).HasWall(Wall.West) ||
							 floor.GetTile(pos - PositiveX).HasWall(Wall.North) ||
							 IsTileImpassable(pos - PositiveX - PositiveZ);
			bool southeast = floor.GetTile(pos + PositiveX + PositiveZ).HasWall(Wall.North) ||
							 floor.GetTile(pos + PositiveX + PositiveZ).HasWall(Wall.West) ||
							 IsTileImpassable(pos + PositiveX + PositiveZ);
			bool southwest = floor.GetTile(pos - PositiveX + PositiveZ).HasWall(Wall.North) ||
							 floor.GetTile(pos + PositiveZ).HasWall(Wall.West) ||
							 IsTileImpassable(pos + PositiveZ - PositiveX);

			if (dir == UnitDirection.North)
				return north;
			if (dir == UnitDirection.West)
				return west;
			if (dir == UnitDirection.South)
				return south;
			if (dir == UnitDirection.East)
				return east;

			if (dir == UnitDirection.NorthEast) {
				return north || east || northeast;
			} else if (dir == UnitDirection.NorthWest) {
				return north || west || northwest;
			} else if (dir == UnitDirection.SouthEast) {
				return south || east || southeast;
			} else if (dir == UnitDirection.SouthWest) {
				return south || west || southwest;
			} else return false;
		}

		public Action GetCurrentAction() => currentAction;
		public void SetCurrentAction(Action action) => currentAction = action;
		public void ClearCurrentAction() => currentAction = currentAction.NextAction();

		public bool AddUnit(Unit unit)
		{
			if (unit == null || IsTileOccupied(unit.position))
				return false;
			units.Add(unit);
			Tile tile = floor.GetTile(unit.position.x, unit.position.z);
			tile.unit = unit;
			floor.SetTile(unit.position, tile);
			return true;
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

		public void PushActionTurnUnit(Unit unit, Position target)
		{
			if (unit != null && currentAction == null) {
				SetCurrentAction(new ActionTurnUnit(this, unit, Pathfinding.GetDirection(unit.position, target)));
			}
		}

		public Unit GetUnit(Position pos)
		{
			return floor.GetTile(pos.x, pos.z).unit;
		}

		// health only for now
		public void AffectUnit(Unit unit, int effect)
		{
			unit.Health += effect;
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
