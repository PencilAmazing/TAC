using TAC.Render;
using TAC.World;

namespace TAC.Editor
{
	class EditorScene : Scene
	{

		public EditorScene(Position size, Renderer renderer, ResourceCache cache)
			: base(size, renderer, cache)
		{

		}

		public void ToggleBrush(Position pos, Wall wall, Brush brush)
		{
			if (wall == Wall.North) {
				if (floor[pos.x, pos.z].North == Brush.nullBrush)
					floor[pos.x, pos.z].North = brush;
				else
					floor[pos.x, pos.z].North = Brush.nullBrush;
				ToggleWall(pos, wall);
			} else if (wall == Wall.West) {
				if (floor[pos.x, pos.z].West == Brush.nullBrush)
					floor[pos.x, pos.z].West = brush;
				else
					floor[pos.x, pos.z].West = Brush.nullBrush;
				ToggleWall(pos, wall);
			}
		}

		public void ToggleWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls ^= (byte)wall;
		}
	}
}
