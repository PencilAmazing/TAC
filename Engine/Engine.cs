using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using TAC.Editor;
using TAC.Render;
using TAC.UISystem;
using TAC.World;
using static ImGuiNET.ImGui;
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

		public void DrawEditUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(new UIEvent(ToggleGameMode));
			}

			PopStyleVar();
			End();
		}

		public void DrawGameUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(new UIEvent(ToggleGameMode));
			}

			PopStyleVar();
			End();
		}

		public void ToggleGameMode() => SetGameMode(!scene.isEdit);

		public void SetGameMode(bool isEdit)
		{
			scene.isEdit = isEdit;
			if (scene.isEdit == true) {
				// Cancel current action
				scene.currentAction = null;
			}
		}

		public void ConvertEditToGame()
		{
			SetGameMode(false);

		}

		public void Shmove(float deltaTime)
		{
			imguiController.Update(deltaTime);
			//ui.Update(deltaTime);
			if (scene.isEdit)
				DrawEditUI(deltaTime);
			else
				DrawGameUI(deltaTime);

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
