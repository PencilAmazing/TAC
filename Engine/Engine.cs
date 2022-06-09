using TAC.Editor;
using TAC.Render;
using TAC.UISystem;
using TAC.World;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace TAC.Inner
{
	public class Engine
	{
		public static int screenWidth = 1000;
		public static int screenHeight = 750;

		public Scene scene;
		public ResourceCache cache;
		public Renderer renderer;
		public UI ui;
		public CameraControl camera;

		private ImguiController imguiController;

		public Engine()
		{
			imguiController = new();
			ui = new();

			imguiController.Load(screenWidth, screenHeight);
			ui.Load();

			cache = new ResourceCache();
			cache.LoadAssets();
			renderer = new Renderer();

			scene = new Scene(new Position(32, 32, 32), renderer, cache, false);

			camera = new CameraControl(scene);
		}

		public void Update(float deltaTime)
		{
			imguiController.Update(deltaTime);
			ui.Update(deltaTime);

			camera.UpdateCamera();
			BeginDrawing();
			ClearBackground(Color.RAYWHITE);
			BeginMode3D(camera.camera);

			if (scene.isEdit)
				camera.UpdateEditControl();
			else
				camera.UpdateGameControl();

			UI.DispatchEvents();

			scene.Think(deltaTime);

			scene.Draw(camera.camera);
			scene.DrawDebug3D(camera.camera);
			EndMode3D();
			scene.DrawDebug();
			scene.DrawUI();
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
