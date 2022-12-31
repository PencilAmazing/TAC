using Raylib_cs;
using System;
using System.Numerics;
using TAC.World;
using static Raylib_cs.CameraProjection;
using static Raylib_cs.KeyboardKey;
using static TAC.UISystem.UI;

namespace TAC.Render
{
	// Should really separate camera and control
	public class CameraControl
	{
		public Camera3D camera;
		public float speed;
		private float sensitivity = 0.01f;
		private Vector2 angle = new(0.0f, MathF.PI / 6);
		private float zoom = 10.0f;

		//private Scene scene;

		public CameraControl(Scene scene, float speed = 0.5f)
		{
			//this.scene = scene;

			camera.position = new Vector3(10.0f, 10.0f, 10.0f);
			camera.target = new Vector3(5.0f, 0.0f, 5.0f);
			camera.up = new Vector3(0.0f, 1.0f, 0.0f);
			camera.fovy = 45.0f;
			camera.projection = CAMERA_PERSPECTIVE;

			Raylib.SetCameraMode(camera, CameraMode.CAMERA_CUSTOM);

			this.speed = speed;
		}

		public void UpdateCamera(Vector3 clamp)
		{
			Vector3 rod = camera.position - camera.target;
			float theta = MathF.Atan2(rod.X, rod.Z);
			Quaternion rot = Raymath.QuaternionFromAxisAngle(camera.up, theta);

			if (IsKeyDown(KEY_W)) {
				camera.target += Raymath.Vector3RotateByQuaternion(-Vector3.UnitZ * speed, rot);
			}
			if (IsKeyDown(KEY_S)) {
				camera.target += Raymath.Vector3RotateByQuaternion(Vector3.UnitZ * speed, rot);
			}
			if (IsKeyDown(KEY_A)) {
				camera.target += Raymath.Vector3RotateByQuaternion(-Vector3.UnitX * speed, rot);
			}
			if (IsKeyDown(KEY_D)) {
				camera.target += Raymath.Vector3RotateByQuaternion(Vector3.UnitX * speed, rot);
			}
			if (IsKeyDown(KEY_Q)) {
				camera.target -= Vector3.UnitY * speed;
			}
			if (IsKeyDown(KEY_E)) {
				camera.target += Vector3.UnitY * speed;
			}
			camera.target = Vector3.Clamp(camera.target, Vector3.Zero, clamp);
			zoom += Raylib.GetMouseWheelMove();

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
				Vector2 mouse = -Raylib.GetMouseDelta();
				// Directly apply mouse delta to angle
				angle += mouse * sensitivity;
				angle.Y = (float)Math.Clamp(angle.Y, MathF.PI / 2 + 0.1, MathF.PI - 0.1);
			}
			camera.position = camera.target - Raymath.Vector3RotateByQuaternion(Vector3.UnitZ * zoom, Raymath.QuaternionFromEuler(angle.Y, angle.X, 0));
		}
	}
}
