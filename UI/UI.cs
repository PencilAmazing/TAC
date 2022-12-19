using ImGuiNET;
using Raylib_cs;
using System.Numerics;
using TAC.Inner;
using TAC.World;
using static ImGuiNET.ImGui;

namespace TAC.UISystem
{
	// Doesn't need to know about scene, scene just inserts info into it
	public class UI
	{
		public ImFontPtr font1;

		public Vector3 clearColor = new Vector3(0.45f, 0.55f, 0.6f);

		public UIEvent SaveFunctionDelegate;
		public UIEvent LoadFunctionDelegate;
		public UIEvent ConvertEditorToLevel;

		public Color GetClearColor()
		{
			return new Color((byte)(clearColor.X * 255), (byte)(clearColor.Y * 255), (byte)(clearColor.Z * 255), (byte)255);
		}

		public void Load() { }

		public void Unload() { }

		public void DrawEditUI(float dt, Engine engine)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(engine.ToggleGameMode);
			}

			PopStyleVar();
			End();
		}

		public void DrawGameUI(float dt, Engine engine)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("controls", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Edit", new Vector2(150, 40))) {
				UIEventQueue.PushEvent(engine.ToggleGameMode);
			}
			End();

			if (engine.player.selectedUnit != null) DrawUnitStats(engine);

			PopStyleVar();
		}

		private void DrawUnitStats(Engine engine)
		{
			PlayerController player = engine.player;
			Unit selectedUnit = player.selectedUnit;

			SetNextWindowPos(Vector2.UnitY * Engine.screenHeight, ImGuiCond.None, Vector2.UnitY);
			Begin("info", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize);
			{
				SetWindowFontScale(1.5f);
				ProgressBar(selectedUnit.TimeUnits / 100.0f, new Vector2(-1, 0), selectedUnit.TimeUnits.ToString());
				// Had to read source code to find the right ImGuiCol. wtf man
				PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(1, 0, 0, 1));
				ProgressBar(selectedUnit.Health / 80.0f, new Vector2(-1, 0), selectedUnit.Health.ToString());
				PopStyleColor();
				Text(selectedUnit.Name);
			}
			End();

			SetNextWindowPos(new Vector2(Engine.screenWidth, Engine.screenHeight), ImGuiCond.None, Vector2.One);
			Begin("Onhand", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
			//SetWindowFontScale(1.5f);	
			if (selectedUnit.inventory.Count > 0) {
				Item item = selectedUnit.inventory[0];
				if (player.mode == PlayerController.GameSelection.SelectTarget) {
					PushStyleColor(ImGuiCol.Button, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.96f, 0.54f, 0.17f, 1));
				}
				UI.ButtonWithCallback(item.name, new Vector2(200, 40), player.StartSelectingTarget);
				if (player.mode == PlayerController.GameSelection.SelectTarget) PopStyleColor(3);
			}
			End();
		}

		public static void DispatchEvents()
		{
			while (UIEventQueue.EventQueue.Count > 0) {
				UIEventDelegate del = UIEventQueue.EventQueue.Dequeue();
				del();
			}
		}

		public static bool IsMouseUnderUI()
		{
			return ImGui.GetIO().WantCaptureMouse;
		}

		/// <summary>
		/// imgui button that calls UIEventQueue.PushQueue when clicked
		/// </summary>
		public static void ButtonWithCallback(string name, Vector2 size, UIEventDelegate callback)
		{
			if (Button(name, size)) UIEventQueue.PushEvent(callback);
		}

		public static bool GetMouseButtonPress(MouseButton button) => !IsMouseUnderUI() && Raylib.IsMouseButtonPressed(button);

		public void Update(float dt)
		{
		}
	}
}
