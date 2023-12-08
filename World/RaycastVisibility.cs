using System.Collections.Generic;
using TAC.Logic;

namespace TAC.World
{
	public partial class Scene
	{
		// Shadowcasting never worked anyways
		// O(n^3) because fuck your maths bro
		public List<Position> GetUnitVisibleTilesRaycast(Position start, UnitDirection dir)
		{
			List<Position> visible = new();
			int visionrange = 4;

			bool TestCell(Position pos)
			{
				return false;
			}

			void IsVisible(Position end)
			{
				if (end == start) return;
				Position[] list = GetSupercoverLine(start, end);
				foreach (Position pos in list) {
					// Flip end and start to get opposite direction
					UnitDirection testDir = Pathfinding.GetDirectionAtan(end, start);
					// Test if this cell can see unit
					// If not, don't add cell
					if (TestDirectionCallback(pos, dir, TestCell)) return;
					// If this is the end cell, break early
					if (pos == end) break;
				}
				visible.Add(end);

			}

			Position diff = Unit.PositionDirections[(int)dir];
			Position limits = diff * visionrange;
			// Adjust limits based on dir
			for (int x = -2; x <= 2; x++)
				for (int y = 0; y <= 0; y++)
					for (int z = -2; z <= 2; z++) {
						IsVisible(start + new Position(x, y, z));
					}

			return visible;
		}
	}
}
