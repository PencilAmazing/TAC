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

		// Neat trick from the openxcom fellas
		Position GenerateVectorFromDirection(UnitDirection dir)
		{
			int[] x = { 0, 1, 1, 1, 0, -1, -1, -1 };
			int[] z = { -1, -1, 0, 1, 1, 1, 0, -1 };
			int[] y = { 0, 0, 0, 0, 0, 0, 0, 0 };
			return new Position(x[(int)dir], y[(int)dir], z[(int)dir]);
		}

		/// <summary>
		/// Finds path from unit to position on grid <br/>
		/// Stores unit and goal inside
		/// </summary>
		/// <returns>True if path is possible</returns>
		public bool FindPathForUnit(Unit unit, Position goal)
		{
			// Skip if tile is occupied
			if (scene.IsTileOccupied(goal))
				return false;
			Position start = unit.position;

			Queue<Position> frontier = new();
			frontier.Enqueue(start);

			Dictionary<Position, Position> camefrom = new();
			camefrom[start] = Position.Negative;

			// Still tiles to explore?
			while (frontier.Count > 0) {
				// Pop off top
				Position current = frontier.Dequeue();
				if (current == goal) break;
				// Fun fact: breadth first is trash if diagonals are involved
				UnitDirection[] dirs =
				{
					UnitDirection.North,
					UnitDirection.East,
					UnitDirection.South,
					UnitDirection.West,
					UnitDirection.NorthEast,
					UnitDirection.NorthWest,
					UnitDirection.SouthWest,
					UnitDirection.SouthEast
				};
				// Check neighbors - beware of boxing!
				foreach (UnitDirection dir in dirs) {
					// For each connected neighbor
					if (!scene.TestDirection(current, dir)) {
						Position next = current + GenerateVectorFromDirection(dir);
						if (!camefrom.ContainsKey(next)) {
							frontier.Enqueue(next);
							camefrom[next] = current;
						}
					}
				}
			}

			// reconstruct path
			Position step = goal;
			while (step != start) {
				path.Add(step);
				step = camefrom[step];
			}
			path.Reverse();

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
