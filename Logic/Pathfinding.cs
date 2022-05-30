using System;
using System.Collections.Generic;
using TAC.World;

namespace TAC.Logic
{
	class Pathfinding
	{
		private Scene scene;

		// Sequence of tiles to reach destination
		public List<Position> path;
		public Unit unit { get; private set; }

		public Pathfinding(Scene scene)
		{
			this.scene = scene;
			this.path = new List<Position>();
		}

		/// <summary>
		/// Finds path from unit to position on grid <br/>
		/// Stores unit and goal inside
		/// </summary>
		/// <returns>True if path is possible</returns>
		public bool FindPathForUnit(Unit unit, Position goal)
		{
			// Skip if tile is occupied
			Tile end = scene.GetTile(goal);
			if (end == Tile.nullTile || end.unit != null)
				return false;

			Tile start = scene.GetTile(unit.position);

			int dx = goal.x - unit.position.x > 0 ? 1 : -1;
			int dz = goal.z - unit.position.z > 0 ? 1 : -1;

			// X axis
			for (int i = unit.position.x; i != goal.x + dx; i += dx) {
				Position pos = new Position(i, 0, unit.position.z);
				Tile tile = scene.GetTile(pos);
				if (tile != Tile.nullTile)
					path.Add(pos);
				else
					return false;
			}

			for (int z = unit.position.z; z != goal.z + dz; z += dz) {
				Position pos = new Position(goal.x, 0, z);
				Tile tile = scene.GetTile(pos);
				if (tile != Tile.nullTile)
					path.Add(pos);
				else
					return false;
			}

			// Store unit and goal for safekeeping
			this.unit = unit;
			return true;
		}

		public static UnitDirection GetDirection(Position from, Position to)
		{
			Position diff = to - from;
			if (diff.z > 0)
				if (diff.x > 0)
					return UnitDirection.NorthEast;
				else if (diff.x < 0)
					return UnitDirection.NorthWest;
				else
					return UnitDirection.North;
			else if (diff.z < 0)
				if (diff.x > 0)
					return UnitDirection.SouthEast;
				else if (diff.x < 0)
					return UnitDirection.SouthWest;
				else
					return UnitDirection.South;
			else {
				if (diff.x > 0) {
					return UnitDirection.East;
				} else if (diff.x < 0) {
					return UnitDirection.West;
				};
			}
			return UnitDirection.North;
		}
	}
}
