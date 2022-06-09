using ImGuiNET;
using Raylib_cs;
using System.Numerics;
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

		public void Load()
		{

		}

		public void Unload()
		{

		}

		public static void DispatchEvents()
		{
			while(UIEventQueue.EventQueue.Count > 0) {
				UIEvent ev = UIEventQueue.EventQueue.Dequeue();
				ev.del();
			}
		}

		public static bool IsMouseUnderUI()
		{
			return ImGui.GetIO().WantCaptureMouse;
		}

		public static bool GetMouseButtonPress(MouseButton button)
		{
			return !IsMouseUnderUI() && Raylib.IsMouseButtonPressed(button);
		}

		public void Update(float dt)
		{
			//PushFont(font1);

			SetNextWindowPos(Vector2.Zero);

			PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
			Begin("huh", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

			if (Button("Play", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(SaveFunctionDelegate);
			}
			SameLine();
			if (Button("Load level", new Vector2(150, 40))) {
				UIEventQueue.EventQueue.Enqueue(LoadFunctionDelegate);
			}

			PopStyleVar();
			//PopFont();
			End();
		}
	}
}
