using ImGuiNET;
using Raylib_cs;
using System;
using System.Numerics;
using TAC.Editor;
using TAC.Inner;
using TAC.Inner.ControllerStates;
using TAC.World;
using static ImGuiNET.ImGui;
using static TAC.Inner.ControllerStates.ControlEditState;
using static TAC.Inner.ControllerStates.ControlGameState;

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

		private int currentItemIndex;
		private readonly string texturePickerLabel = "Texture picker popup";
		private readonly string tilePickerLabel = "Tile picker popup";
		private string editTextureName;
		private string newBrushName;

		private Engine engine;

		public UI(Engine engine)
		{
			this.engine = engine;
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

			BeginGroup();
			// Piss off I'm using reflection here
			foreach (ToolType type in Enum.GetValues(typeof(ToolType)))
				if (RadioButton(type.ToString(), engine.player.EditState.SelectedTool == type))
					engine.player.EditState.SelectedTool = type;
			EndGroup();

			End();

			switch (engine.player.EditState.SelectedTool) {
				case ToolType.None:
					break;
				case ToolType.Wall:
					DrawMaterialSelectionPanel();
					break;
				case ToolType.Tile:
					DrawTileSelectionPanel();
					break;
				case ToolType.Object:
					DrawObjectSelectionPanel();
					break;
				case ToolType.Unit:
					break;
				default:
					break;
			}
			DrawWallInfo();

			PopStyleVar();
		}

		private void DrawWallInfo()
		{
			Begin("Wall info", ImGuiWindowFlags.AlwaysAutoResize);
			PlayerController player = engine.player;

			Position pos = player.GetMouseTilePosition();
			Tile tile = engine.scene.GetTile(pos);
			Text("Tile location: " + pos.ToString());
			Text("Wall data: " + Convert.ToString(tile.walls, 2).PadLeft(4, '0'));
			Text("Tile type: " + tile.type.ToString());
			Text("Tile thing: " + tile.thing.ToString());
			End();
		}

		private void DrawObjectSelectionPanel()
		{
			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.One * 5);
			Begin("Object selection panel");

			End();
		}

		private void DrawTileSelectionPanel()
		{
			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.One * 5);

			Begin("Tile selection panel", ImGuiWindowFlags.AlwaysAutoResize);
			Texture selectedTile = engine.scene.TileTypeMap[engine.player.EditState.SelectedTileIndex];

			BeginGroup();
			Image((IntPtr)selectedTile.texture.id, new Vector2(128, 128));

			if (InputText("", ref editTextureName, (uint)256) || IsItemClicked()) {
				// New texture inputted
				Texture newTex = engine.scene.cache.GetTexture(editTextureName);
				if (newTex != null) {
					// Bind new texture to selection
					engine.player.EditState.SelectedTileIndex = engine.scene.GetTileTypeIndexOf(newTex);
				}
			}
			Separator();
			// Add quick option to select new tile types
			if (Button("Select tile", new Vector2(128, 0))) OpenPopup(tilePickerLabel);
			if (BeginPopup(tilePickerLabel)) {
				DrawTileSelectionPanel(ref engine.player.EditState.SelectedTileIndex);
			}

			// List all already loaded tile types
			for (int i = 0; i < engine.scene.TileTypeMap.Count; i++) {
				if (Selectable(engine.scene.TileTypeMap[i].assetname, false, ImGuiSelectableFlags.None, new Vector2(128, 0))) {
					engine.player.EditState.SelectedTileIndex = i;
				}
			}

			EndGroup();
			SameLine();
			// Snap level selection to allow editing air
			string sliderFormat = engine.player.EditState.ForceYLevelEdit == 0 ? "x" : "%i";
			VSliderInt("##Level Selection", new Vector2(20, 200), ref engine.player.EditState.ForceYLevelEdit,
				0, engine.scene.Size.y, sliderFormat, ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.NoInput);

			End();
			PopStyleVar();
		}

		private void DrawTileSelectionPanel(ref int outTileIndex)
		{
			foreach (Texture tex in engine.resourceCache.Textures.Values) {
				// Filter only tiles
				if (!tex.assetname.StartsWith("tile/")) continue;
				if (Selectable(tex.assetname)) {
					outTileIndex = engine.scene.GetTileTypeIndexOf(tex);
					CloseCurrentPopup();
				}
			}
			EndPopup();
		}

		private void DrawTextureSelectionPanel(Brush brush, int editFace)
		{
			foreach (Texture tex in engine.resourceCache.Textures.Values) {
				//if (!tex.assetname.StartsWith("Tile")) continue;
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
			//SetNextWindowSize(-Vector2.One);
			if (Begin("Material Panel", ImGuiWindowFlags.NoCollapse|ImGuiWindowFlags.AlwaysAutoResize)) {
				if (BeginTable("Material Panel Content", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit)) {
					TableNextColumn();
					foreach (string key in engine.resourceCache.Brushes.Keys) {
						Brush brush = engine.resourceCache.Brushes[key];
						if (Button(brush.assetname, Vector2.UnitX * 100)) {
							// Attempt to set selected brush
							engine.player.EditState.SelectedBrush = brush;
						}
					}

					TableNextColumn();
					Brush current = engine.player.EditState.SelectedBrush;
					if (current != null) {
						BeginGroup();
						{
							Text("Brush: " + current.assetname);
							SameLine();
							Checkbox("Flip", ref engine.player.EditState.FlipBrush);
						}
						EndGroup();

						SetNextItemWidth(280);
						if (BeginListBox("##TexturesListBox")) {
							for (int i = 0; i < current.faces.Length; i++) {
								PushID(i);
								BeginGroup();
								editTextureName = current.faces[i] != null ? current.faces[i].assetname : "";
								// FIXME Magic number lol
								SetNextItemWidth(150);
								if (InputText(Brush.FaceLabels[i], ref editTextureName, (uint)256) || IsItemClicked()) {
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
								EndGroup();
								PopID();
							}
						}
						EndListBox();

						TableNextColumn();
						// Draw preview of selected texture
						if (current.faces[currentItemIndex] != null)
							Image((IntPtr)current.faces[currentItemIndex].texture.id, Vector2.One * 128);
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
						engine.player.EditState.SelectedBrush = newBrush;
					}
					SameLine();
					InputText("", ref newBrushName, (uint)256);
					SameLine();
					//if (Button("Close")) ShowMaterialPanel = false;
					EndGroup();
				}
				End();
			}
			PopStyleVar();
		}

		public void DrawGameUI(float dt)
		{
			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.One);

			ImGuiWindowFlags controlFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize;

			if (IsPlayerTeamInPlay(engine.player)) {
				////////////////
				SetNextWindowPos(Vector2.Zero);
				if (engine.scene.CurrentActionInProgress()) BeginDisabled();
				Begin("Game Controls", controlFlags);
				if (Button("Next turn", new Vector2(150, 40))) {
					engine.scene.EndTurn();
				}
				End();
				////////////////
				SetNextWindowPos(new Vector2(GetWindowViewport().Size.X - 150, 0));
				Begin("Hidden Controls", controlFlags);
				if (Button("Edit", new Vector2(150, 40)))
					UIEventQueue.PushEvent(engine.ToggleGameMode);
				End();
				if (engine.scene.CurrentActionInProgress()) EndDisabled();
				////////////////
				if (engine.player.SelectedUnit != null)
					DrawUnitStats();
			} else {
				// Draw waiting UI
				Vector2 windowPos = GetMainViewport().Size;
				string TeamInPlayName = "Waiting for " + engine.scene.GetCurrentTeamInPlay().Name;

				PushStyleColor(ImGuiCol.Text, ColorConvertFloat4ToU32(new Vector4(1, 0.4f, 0.2f, 1)));
				Vector2 TextSize = CalcTextSize(TeamInPlayName);
				windowPos.X = (windowPos.X - TextSize.X) / 2;
				windowPos.Y -= GetTextLineHeightWithSpacing() * 2;
				SetNextWindowPos(windowPos);
				Begin("Waiting", controlFlags);
				SetWindowFontScale(2.0f); // IDGAF it works bro
				Text(TeamInPlayName);
				End();
				PopStyleColor();
			}

			PopStyleVar();
		}

		private void DrawUnitStats()
		{
			PlayerController player = engine.player;
			Unit selectedUnit = player.SelectedUnit;
			UnitTemplate template = selectedUnit.Template;

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
				if (player.SelectionMode == GameSelection.SelectTarget) {
					PushStyleColor(ImGuiCol.Button, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.96f, 0.54f, 0.17f, 1));
					PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.96f, 0.54f, 0.17f, 1));
				}
				UI.ButtonWithCallback(item.name, new Vector2(200, 40), player.StartSelectingTarget);
				if (player.SelectionMode == GameSelection.SelectTarget) PopStyleColor(3);
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

		private bool IsPlayerTeamInPlay(PlayerController player) => engine.scene.IsTeamInPlay(player.GameState.SelectedTeam);

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
