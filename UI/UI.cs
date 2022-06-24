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

		public void Load() { }

		public void Unload() { }

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

		public static void ButtonWithCallback(string name, Vector2 size, UIEventDelegate callback)
		{
			if (Button(name, size)) {
				UIEventQueue.PushEvent(callback);
			}
		}

		public static bool GetMouseButtonPress(MouseButton button)
		{
			return !IsMouseUnderUI() && Raylib.IsMouseButtonPressed(button);
		}

		public void Update(float dt)
		{
			return;
		}
	}
}
