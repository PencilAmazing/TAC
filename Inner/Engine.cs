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
		public ResourceCache resourceCache;
		public Renderer renderer;
		public UI ui;
		public PlayerController player;

		private ImguiController imguiController;

		public Engine()
		{
			imguiController = new();
			ui = new();

			imguiController.Load(screenWidth, screenHeight);
			ui.Load();

			resourceCache = new ResourceCache();
			resourceCache.LoadAssets();
			renderer = new Renderer();

			scene = new Scene(new Position(32, 32, 32), renderer, resourceCache, false);
			player = new PlayerController(scene);
		}

		public void DrawEditUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(ToggleGameMode);
			}

			PopStyleVar();
			End();
		}

		// Move this to UI class for god's sake
		public void DrawGameUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("controls", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Edit", new Vector2(150, 40))) {
				UIEventQueue.PushEvent(ToggleGameMode);
			}
			End();

			if (player.selectedUnit != null) {
				SetNextWindowPos(Vector2.UnitY * screenHeight, ImGuiCond.None, Vector2.UnitY);
				Begin("info", ImGuiWindowFlags.NoDecoration|ImGuiWindowFlags.AlwaysAutoResize);
				SetWindowFontScale(1.5f);
				ProgressBar((float)player.selectedUnit.TimeUnits / 100.0f, new Vector2(-1,0), player.selectedUnit.TimeUnits.ToString());
				Text(player.selectedUnit.Name);
				End();

				SetNextWindowPos(new Vector2(screenWidth, screenHeight), ImGuiCond.None, Vector2.One);
				Begin("Onhand", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
				//SetWindowFontScale(1.5f);
				if (player.selectedUnit.inventory.Count > 0) {
					Item item = player.selectedUnit.inventory[0];
					UI.ButtonWithCallback(item.name, new Vector2(200, 40), player.StartSelectingTarget);
				}

				End();
			}

			PopStyleVar();
		}

		public void ToggleGameMode() => SetGameMode(!scene.isEdit);

		public void SetGameMode(bool isEdit)
		{
			scene.isEdit = isEdit;
			if (scene.isEdit == true && scene.GetCurrentAction() != null) {
				// Cancel current action
				scene.ClearCurrentAction();
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

			player.camera.UpdateCamera(scene.size.ToVector3());
			BeginDrawing();
			ClearBackground(Color.RAYWHITE);
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
