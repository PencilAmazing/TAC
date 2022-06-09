using TAC.Editor;
using TAC.Render;
using TAC.World;
using TAC.UISystem;
using static Raylib_cs.Color;
using static Raylib_cs.Raylib;

namespace TAC
{
	public class Game
	{
		public static int Main()
		{
			const int screenWidth = 1000;
			const int screenHeight = 750;

			// Init opengl to make things easier
			InitWindow(screenWidth, screenHeight, "bideo game");

			ImguiController imguiController = new();
			UI editor = new();

			imguiController.Load(screenWidth, screenHeight);
			editor.Load();

			bool isEdit = true;

			ResourceCache db = new ResourceCache();
			db.LoadAssets();
			Renderer renderer = new Renderer();

			Scene scene;
			if (isEdit) {
				scene = new EditorScene(new Position(32, 32, 32), renderer, db);
				(scene as EditorScene).ToggleBrush(new Position(0, 0, 0), Wall.North, 1);
				(scene as EditorScene).ToggleBrush(new Position(0, 0, 0), Wall.West, 1);
			} else {
				scene = new GameScene(new Position(32, 32, 32), renderer, db);
				(scene as GameScene).AddUnit(new Unit(0, new Position(0, 0, 0), "Bruh-bot 9001", UnitDirection.North));
			}

			//editor.SaveFunctionDelegate = new UIEvent(db.WriteSceneToDisk);

			CameraControl camera = new CameraControl(scene);
			SetTargetFPS(60);
			while (!WindowShouldClose()) // Detect window close button or ESC key
			{
				imguiController.Update(GetFrameTime());
				editor.Update(GetFrameTime());

				camera.UpdateCamera();

				BeginDrawing();
				ClearBackground(RAYWHITE);
				BeginMode3D(camera.camera);

				if (isEdit)
					camera.UpdateEditControl();
				else
					camera.UpdateGameControl();

				UI.DispatchEvents();

				scene.Think(GetFrameTime());

				scene.Draw(camera.camera);
				scene.DrawDebug3D(camera.camera);
				EndMode3D();
				scene.DrawDebug();
				scene.DrawUI();
				imguiController.Draw();
				EndDrawing();
			}

			editor.Unload();
			imguiController.Dispose();
			CloseWindow();
			return 0;
		}
	}
}