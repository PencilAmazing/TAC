using Raylib_cs;
using TAC.Editor;
using TAC.Inner;
using TAC.Logic;
using TAC.Render;
using TAC.World;
using static Raylib_cs.Raylib;

namespace TAC
{
	public class Game
	{
		private static Engine engine;

		private static void LoadScene()
		{
			// Load scene geometry and textures
			Brush coppperBrush = engine.resourceCache.LoadBrush("brush/copper");
			// No need to add this, gets added automatically
			Brush yellowBrush = engine.resourceCache.LoadBrush("brush/yellow");
			engine.scene.AddBrushToMap(coppperBrush); // Should be done during scene load

			Texture transparentTexture;
			{
				Image i = GenImageColor(128, 128, Color.BLANK);
				Texture2D tex = LoadTextureFromImage(i);
				UnloadImage(i);
				transparentTexture = engine.resourceCache.LoadTexture(tex, "tile/null");
			}
			Texture floorTexture = engine.resourceCache.LoadTexture("tile/OBKMTB90");
			engine.scene.AddTileToMap(transparentTexture);
			engine.scene.AddTileToMap(floorTexture);
			{
				Thing thing = engine.resourceCache.LoadThing("thing/bookshelf");
				engine.scene.AddThingToMap(thing);
			}
			engine.scene.SetTileSpace(new SceneTileSpace(new Position(16, 2, 16)));

			// Load items and effects
			Sprite impactEffect = new Sprite(engine.resourceCache.GetTexture("scene/sprite/explosion_11"), 6, 32, 32);
			Sprite actionEffect = new Sprite(engine.resourceCache.GetTexture("scene/sprite/ProjectileArranged"), 6, 256, 64);
			Item stick = new Item("Stick", 2, impactEffect, actionEffect);

			// Load unit templates and teams
			UnitTemplate template = engine.resourceCache.GetUnitTemplate("unit/mech");
			// These would be stored in a file somewhere
			Team playerTeam = new Team("Embuscade", false);
			Team enemyTeam = new Team("Welcome Party", true);

			engine.scene.AddTeam(playerTeam);
			engine.scene.AddTeam(enemyTeam);

			engine.player.GameState.SelectedTeam = playerTeam;

			// Create/Load units themselves
			Unit BruhBot = new Unit(template, new Position(0, 0, 0), "Bruh-bot 9001");
			BruhBot.AddToInventory(stick);
			engine.scene.AddUnit(BruhBot, playerTeam);

			Unit PoorFella = new Unit(template, new Position(2, 0, 5), "Poor fella");
			engine.scene.AddUnit(PoorFella, enemyTeam);
			PoorFella.UnitAI = new UnitAIModule(engine.scene, PoorFella);

			Unit AnotherFella = new Unit(template, new Position(6, 0, 1), "Another fool");
			engine.scene.AddUnit(AnotherFella, enemyTeam);
			AnotherFella.UnitAI = new UnitAIModule(engine.scene, AnotherFella);
		}

		public static int Main()
		{
			// Init opengl to make things easier
			InitWindow(Engine.screenWidth, Engine.screenHeight, "bideo game");

			engine = new Engine();

			LoadScene();

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