﻿using TAC.Editor;
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

		public Scene scene { get; private set; }
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

			//SetScene(new Scene(renderer, resourceCache, false));
			//scene = new Scene(renderer, resourceCache, false);
			//player = new PlayerController(scene);
		}

		public void SetScene(Scene scene)
		{
			this.scene = scene;
			this.scene.renderer = this.renderer;
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

			player.camera.UpdateCamera(scene.Size.ToVector3());
			BeginDrawing();
			ClearBackground(Raylib_cs.Color.RAYWHITE);
			BeginMode3D(player.camera.camera);

			if (scene.isEdit)
				player.UpdateEditControl();
			else if (scene.IsTeamInPlay(player.GameState.SelectedTeam)) {
				player.UpdateGameControl();
			}

			UI.DispatchEvents();
			scene.Think(deltaTime);

			scene.Draw(player.camera.camera);
			scene.DrawDebug3D(player.camera.camera);
			EndMode3D();

			// Has to be called outside of 3D mode loop because this
			// changes render targets to another render texture
			// hopefully is unnecessary with GPU based atlas rendering
			scene.UpdateTileSpace();

			imguiController.Draw();
			EndDrawing();
		}

		public void Shutdown()
		{
			ui.Unload();
			imguiController.Dispose();
			resourceCache.WriteSceneToDisk(scene, "default");
			foreach(Brush brush in resourceCache.Brushes.Values) {
				if (!Brush.IsBrushValid(brush)) continue;
				if (!resourceCache.AssetExists(brush.assetname + ".json")) {
					resourceCache.WriteBrushToDisk(brush);
				}

			}
		}
	}
}
