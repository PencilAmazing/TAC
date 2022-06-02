﻿using System.IO;
using TAC.Render;
using TAC.Editor;
using TAC.World;
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
			InitWindow(screenWidth, screenHeight, Directory.GetCurrentDirectory());

			bool isEdit = true;

			ResourceCache db = new ResourceCache();
			db.LoadAssets();

			Renderer renderer = new Renderer();
			Scene scene;
			if (isEdit)
				scene = new EditorScene(new Position(32, 32, 32), renderer, db);
			else
				scene = new GameScene(new Position(32, 32, 32), renderer, db);

			// Define the camera to look into our 3d world
			CameraControl camera = new CameraControl(scene);

			if (!isEdit)
				(scene as GameScene).AddUnit(new Unit(0, new Position(0, 0, 0), "Bruh-bot 9001", UnitDirection.East));
			else
				(scene as EditorScene).ToggleWall(new Position(0, 0, 0), Wall.West);

			SetTargetFPS(60);

			while (!WindowShouldClose()) // Detect window close button or ESC key
			{
				camera.UpdateCamera();

				BeginDrawing();
				ClearBackground(RAYWHITE);
				BeginMode3D(camera.camera);

				if (isEdit)
					camera.UpdateEditControl();
				else
					camera.UpdateGameControl();
				scene.Think(GetFrameTime());

				scene.Draw(camera.camera);
				scene.DrawDebug3D(camera.camera);
				EndMode3D();
				scene.DrawDebug();
				EndDrawing();
			}

			CloseWindow();
			return 0;
		}
	}
}