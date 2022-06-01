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

		public void ToggleWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls ^= (byte)wall;
		}
	}
}
