using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using TAC.Render;
using TAC.Editor;
using TAC.Logic;

namespace TAC.World
{
	public class Scene
	{
		public List<Unit> units;

		public Floor floor;

		public ResourceCache cache;
		public Renderer renderer { get; }
		public Position size { get; }
		public bool isEdit;

		private Stack<DebugText> debugStack;

		public Action currentAction;

		public Scene(Position size, Renderer renderer, ResourceCache cache, bool isEdit)
		{
			this.size = size;
			this.cache = cache;
			this.renderer = renderer;
			this.units = new();
			this.isEdit = isEdit;
			currentAction = null;

			floor = new Floor(size.x, size.y);
			floor.CreateTexture(cache);

			debugStack = new Stack<DebugText>();
		}

		public virtual void Think(float deltaTime)
		{
			foreach (Unit unit in units)
				unit.Think(deltaTime);

			if (currentAction != null) {
				currentAction.Think(deltaTime);
				if (currentAction.isDone) currentAction = null;
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
			ActionMoveUnit action = currentAction as ActionMoveUnit;
			if (action != null)
				renderer.DrawDebugPath(action.path.path.ToArray());
		}
		public virtual void DrawUI() { return; }

		public Model GetFloorQuad()
		{
			return floor.GetQuad();
		}

		public Tile GetTile(Position pos)
		{
			return floor.GetTile(pos.x, pos.z);
		}

		public bool IsTileOccupied(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile == Tile.nullTile || tile.unit != null; // Or tile has object
		}

		public void ToggleWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls ^= (byte)wall;
		}

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

		public void AddUnit(Unit unit)
		{
			if (unit == null)
				return;
			units.Add(unit);
			Tile tile = floor.GetTile(unit.position.x, unit.position.z);
			tile.unit = unit;
			floor.SetTile(unit.position, tile);
		}

		public void MoveUnit(Unit unit, Position goal)
		{
			if (unit == null) return;
			Pathfinding path = new Pathfinding(this);
			if (path.FindPathForUnit(unit, goal)) {
				currentAction = new ActionMoveUnit(this, path);
			}
		}

		public Unit GetUnit(Position pos)
		{
			return floor.GetTile(pos.x, pos.z).unit;
		}
	}
}
