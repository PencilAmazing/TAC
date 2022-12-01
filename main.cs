using TAC.Inner;
using TAC.Render;
using TAC.World;
using static Raylib_cs.Raylib;

namespace TAC
{
	public class Game
	{
		public static int Main()
		{
			// Init opengl to make things easier
			InitWindow(Engine.screenWidth, Engine.screenHeight, "bideo game");

			Engine engine = new Engine();

			if (engine.scene.isEdit) {
				engine.scene.ToggleBrush(new Position(0, 0, 0), Wall.North, 1);
				engine.scene.ToggleBrush(new Position(0, 0, 0), Wall.West, 1);
			} else {
				engine.scene.AddUnit(new Unit(0, new Position(0, 0, 0), "Bruh-bot 9001", UnitDirection.North));
				// This all should be loaded by cache from 
				Sprite impactEffect = new Sprite(0, 6, 32, 32);
				Sprite actionEffect = new Sprite(1, 6, 256, 64);
				Item stick = new Item("Stick", 2, impactEffect, actionEffect);

				engine.scene.units[0].AddToInventory(stick);
			}

			//editor.SaveFunctionDelegate = new UIEvent(db.WriteSceneToDisk);

			SetTargetFPS(60);
			while (!WindowShouldClose()) // Detect window close button or ESC key
			{
				float dt = GetFrameTime();
				engine.Shmove(dt);
			}

			engine.Shutdown();
			CloseWindow();
			return 0;
		}
	}
}