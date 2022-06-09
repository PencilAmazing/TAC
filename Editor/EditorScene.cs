using TAC.Render;
using TAC.World;
using ImGuiNET;
using TAC.UISystem;
using static ImGuiNET.ImGui;
using System.Numerics;

namespace TAC.Editor
{
	class EditorScene : Scene
	{
		public EditorScene(Position size, Renderer renderer, ResourceCache cache)
			: base(size, renderer, cache)
		{
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

		public void ToggleWall(Position pos, Wall wall)
		{
			floor[pos.x, pos.z].walls ^= (byte)wall;
		}

		public GameScene ConvertToGame()
		{
			GameScene scene = new GameScene(this.size, this.renderer, this.cache);
			scene.units = this.units;
			scene.floor = this.floor;
			return scene;
		}
	}
}
