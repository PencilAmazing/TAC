using TAC.Editor;
using TAC.Render;
using TAC.UISystem;
using TAC.World;
using static Raylib_cs.Raylib;

namespace TAC.Inner
{
	public class Engine
	{
		public static int screenWidth = 1000;
		public static int screenHeight = 750;

		public Scene scene;
		public ResourceCache resourceCache;
		public Renderer renderer;
		public UI ui;
		public PlayerController player;

		private ImguiController imguiController;

		public Engine()
		{
			imguiController = new();
			ui = new UI(this);

			imguiController.Load(screenWidth, screenHeight);
			ui.Load();

			resourceCache = new ResourceCache();
			resourceCache.LoadAssets();
			renderer = new Renderer();

			scene = new Scene(new Position(32, 32, 32), renderer, resourceCache, false);
			player = new PlayerController(scene);
		}

		public void ToggleGameMode() => SetGameMode(!scene.isEdit);

		public void SetGameMode(bool isEdit)
		{
			scene.isEdit = isEdit;
			if (scene.isEdit == true && scene.GetCurrentAction() != null) {
				// Cancel current action
				// FIXME maybe call Done() beforehand to allow cleanup?
				scene.ClearCurrentAction();
			}
		}

		public void Shmove(float deltaTime)
		{
			imguiController.Update(deltaTime);
			//ui.Update(deltaTime);
			ui.DrawHUD(deltaTime, scene.isEdit);

			player.camera.UpdateCamera(scene.size.ToVector3());
			BeginDrawing();
			ClearBackground(Raylib_cs.Color.RAYWHITE);
			BeginMode3D(player.camera.camera);

			if (scene.isEdit)
				player.UpdateEditControl();
			else
				player.UpdateGameControl();

			UI.DispatchEvents();
			scene.Think(deltaTime);

			scene.Draw(player.camera.camera);
			scene.DrawDebug3D(player.camera.camera);
			EndMode3D();

			scene.DrawDebug();
			//scene.DrawUI();
			imguiController.Draw();
			EndDrawing();
		}

		public void Shutdown()
		{
			ui.Unload();
			imguiController.Dispose();
		}
	}
}
