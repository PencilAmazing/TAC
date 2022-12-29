using ImGuiNET;
using Raylib_cs;
using System;
using System.Numerics;
using TAC.Editor;
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

		private bool ShowMaterialPanel;
		private int currentItemIndex;
		private readonly string texturePickerLabel = "Texture picker popup";
		private string editTextureName;
		private string newBrushName;

		private Engine engine;

		public UI(Engine engine)
		{
			this.engine = engine;
			ShowMaterialPanel = true;
			currentItemIndex = 0;
			editTextureName = "";
			newBrushName = "";
		}

		public Color GetClearColor()
		{
			return new Color((byte)(clearColor.X * 255), (byte)(clearColor.Y * 255), (byte)(clearColor.Z * 255), (byte)255);
		}

		public void Load() { }

		public void Unload() { }

		public void DrawHUD(float dt, bool isEdit)
		{
			if (isEdit)
				DrawEditUI(dt);
			else
				DrawGameUI(dt);
		}

		public void DrawEditUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);
			SetNextWindowSize(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(-1, 40))) {
				UIEventQueue.EventQueue.Enqueue(engine.ToggleGameMode);
			}
			if (Button("Material Selection", new Vector2(0, 50))) {
				ShowMaterialPanel = !ShowMaterialPanel;
			}
			End();
			if (ShowMaterialPanel) DrawMaterialSelectionPanel();

			DrawWallInfo();

			PopStyleVar();
		}

		private void DrawWallInfo()
		{
			Begin("Wall info");
			PlayerController player = engine.player;

			Position pos = player.GetMouseTilePosition();
			Tile tile = engine.scene.GetTile(pos);
			Text("Tile location: " + pos.ToString());
			Text("Wall data: " + Convert.ToString(tile.walls, 2).PadLeft(4, '0'));
			End();
		}

		private void DrawTextureSelectionPanel(Brush brush, int editFace)
		{
			foreach (Texture tex in engine.resourceCache.Textures.Values) {
				if (Selectable(tex.assetname)) {
					brush.faces[editFace] = tex;
					CloseCurrentPopup();
				}
			}
			EndPopup();
		}

		private void DrawMaterialSelectionPanel()
		{
			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.One * 5);
			SetNextWindowSize(-Vector2.One);
			if (Begin("Material Panel", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)) {
				if (BeginTable("Material Panel Content", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit)) {
					TableNextColumn();
					foreach (string key in engine.resourceCache.Brushes.Keys) {
						Brush brush = engine.resourceCache.Brushes[key];
						if (Button(brush.assetname, Vector2.UnitX * 100)) {
							// Attempt to set selected brush
							engine.player.EditState.selectedBrush = brush;
						}
					}

					TableNextColumn();
					Brush current = engine.player.EditState.selectedBrush;
					if (current != null) {
						//int currentItemIndex = 0;
						BeginGroup();
						Text("Brush: " + current.assetname);
						SameLine();
						Checkbox("Flip", ref engine.player.EditState.FlipBrush);
						EndGroup();

						BeginGroup();
						if (BeginListBox("##TexturesListBox", -Vector2.UnitX)) {
							for (int i = 0; i < current.faces.Length; i++) {
								PushID(i);
								//PushItemWidth(-1);
								editTextureName = current.faces[i] != null ? current.faces[i].assetname : "";
								// Magic number lol
								if (InputText("", ref editTextureName, (uint)256) || IsItemClicked()) {
									currentItemIndex = i;
									// New texture inputted
									Texture newTex = engine.scene.cache.GetTexture(editTextureName);
									if (newTex != null) {
										current.faces[i] = newTex;
									}
								}
								SameLine();
								if (Button("pick", Vector2.UnitX * 50)) OpenPopup(texturePickerLabel);
								if (BeginPopup(texturePickerLabel)) DrawTextureSelectionPanel(current, i);
								//PopItemWidth();
								PopID();
							}
						}
						EndGroup();
						EndListBox();

						TableNextColumn();
						if (current.faces[currentItemIndex] != null)
							Image((IntPtr)current.faces[currentItemIndex].tex.id, Vector2.One * 128);
					}
					EndTable();

					Spacing();
					Separator();
					BeginGroup();
					if (Button("New Brush") && !String.IsNullOrWhiteSpace(newBrushName)) {
						newBrushName = newBrushName.Replace(" ", string.Empty);
						// TODO wtf
						Brush newBrush = new Brush("brush/" + newBrushName, new Texture[6]);
						engine.resourceCache.Brushes.Add(newBrush.assetname, newBrush);
						engine.player.EditState.selectedBrush = newBrush;
					}
					SameLine();
					InputText("", ref newBrushName, (uint)256);
					SameLine();
					if (Button("Close")) ShowMaterialPanel = false;
					EndGroup();
				}
				End();
			}
			PopStyleVar();
		}

		public void DrawGameUI(float dt)
		{
			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("controls", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Edit", new Vector2(150, 40))) {
				UIEventQueue.PushEvent(engine.ToggleGameMode);
			}
			End();

			if (engine.player.SelectedUnit != null) DrawUnitStats();

			PopStyleVar();
		}

		private void DrawUnitStats()
		{
			PlayerController player = engine.player;
			Unit selectedUnit = player.SelectedUnit;
			UnitTemplate template = selectedUnit.Type;

			SetNextWindowPos(Vector2.UnitY * Engine.screenHeight, ImGuiCond.None, Vector2.UnitY);
			Begin("info", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize);
			{
				SetWindowFontScale(1.5f);
				ProgressBar((float)selectedUnit.TimeUnits / template.TimeUnits, new Vector2(-1, 0), selectedUnit.TimeUnits.ToString());
				// Had to read source code to find the right ImGuiCol. wtf man
				PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(1, 0, 0, 1));
				ProgressBar((float)selectedUnit.Health / template.Health, new Vector2(-1, 0), selectedUnit.Health.ToString());
				PopStyleColor();
				Text(selectedUnit.Name);
			}
			End();

			SetNextWindowPos(new Vector2(Engine.screenWidth, Engine.screenHeight), ImGuiCond.None, Vector2.One);
			Begin("Onhand", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
			//SetWindowFontScale(1.5f);	
			if (selectedUnit.inventory.Count > 0) {
				Item item = selectedUnit.inventory[0];
				if (player.SelectionMode == PlayerController.GameSelection.SelectTarget) {
					PushStyleColor(ImGuiCol.Button, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.96f, 0.54f, 0.17f, 1));
				}
				UI.ButtonWithCallback(item.name, new Vector2(200, 40), player.StartSelectingTarget);
				if (player.SelectionMode == PlayerController.GameSelection.SelectTarget) PopStyleColor(3);
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

		public static bool IsMouseUnderUI() => ImGui.GetIO().WantCaptureMouse;

		// WantCaptureKeyboard is not always accurate, pray to god you don't need it
		public static bool IsKeyboardCapturedByUI() => ImGui.GetIO().WantTextInput;

		/// <summary>
		/// imgui button that calls UIEventQueue.PushQueue when clicked
		/// Mostly for game affecting events
		/// </summary>
		public static void ButtonWithCallback(string name, Vector2 size, UIEventDelegate callback)
		{
			if (Button(name, size)) UIEventQueue.PushEvent(callback);
		}

		public static bool GetMouseButtonPress(MouseButton button) => !IsMouseUnderUI() && Raylib.IsMouseButtonPressed(button);
		public static bool IsKeyDown(KeyboardKey key) => !IsKeyboardCapturedByUI() && (bool)Raylib.IsKeyDown(key);

		public void Update(float dt)
		{
		}
	}
}
