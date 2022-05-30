using Raylib_cs;
using System.Numerics;
using static Raylib_cs.CameraProjection;
using static Raylib_cs.KeyboardKey;
using static Raylib_cs.Raylib;
using System;
using TAC.Render;
using TAC.World;

namespace TAC.Render
{
	class CameraControl
	{
		public Camera3D camera;
		public float speed;
		private float sensitivity = 0.01f;
		private Vector2 angle;
		private float zoom = 10.0f;

		private Unit selectedUnit;
		private GameScene scene;

		public CameraControl(GameScene scene, float speed = 0.5f)
		{
			this.scene = scene;

			camera.position = new Vector3(10.0f, 10.0f, 10.0f);
			camera.target = new Vector3(5.0f, 0.0f, 5.0f);
			camera.up = new Vector3(0.0f, 1.0f, 0.0f);
			camera.fovy = 45.0f;
			camera.projection = CAMERA_PERSPECTIVE;

			SetCameraMode(camera, CameraMode.CAMERA_CUSTOM);
			//SetCameraMoveControls(KeyboardKey.KEY_W, KeyboardKey.KEY_S, KeyboardKey.KEY_D, KeyboardKey.KEY_A, KeyboardKey.KEY_Q, KeyboardKey.KEY_E);

			this.speed = speed;
			selectedUnit = null;
		}

		public void UpdateCamera()
		{
			Vector3 rod = camera.position - camera.target;
			Vector2 angleV = Raymath.Vector3Angle(rod, Vector3.UnitY);
			Quaternion rot = Quaternion.CreateFromAxisAngle(camera.up, angleV.X);

			if (IsKeyDown(KEY_W)) {
				camera.target += Raymath.Vector3RotateByQuaternion(Vector3.UnitZ * speed, rot);
			}
			if (IsKeyDown(KEY_S)) {
				camera.target += Raymath.Vector3RotateByQuaternion(-Vector3.UnitZ * speed, rot);
			}
			if (IsKeyDown(KEY_A)) {
				camera.target += Raymath.Vector3RotateByQuaternion(Vector3.UnitX * speed, rot);
			}
			if (IsKeyDown(KEY_D)) {
				camera.target += Raymath.Vector3RotateByQuaternion(-Vector3.UnitX * speed, rot);
			}
			if (IsKeyDown(KEY_Q)) {
				camera.target -= Vector3.UnitY * speed;
			}
			if (IsKeyDown(KEY_E)) {
				camera.target += Vector3.UnitY * speed;
			}
			camera.target = Vector3.Clamp(camera.target, Vector3.Zero, scene.floor.size);
			zoom += GetMouseWheelMove();

			if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)) {
				Vector2 mouse = -GetMouseDelta();
				angle += mouse * sensitivity;
				angle.Y = (float)Math.Clamp(angle.Y, Math.PI / 2 + 0.1, Math.PI - 0.1);
			}
			camera.position = camera.target - Raymath.Vector3RotateByQuaternion(Vector3.UnitZ * zoom, Raymath.QuaternionFromEuler(angle.Y, angle.X, 0));
		}

		private Position GetMouseTilePosition()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera);
			// replace this with quad collision instead
			RayCollision collide = GetRayCollisionModel(ray, scene.GetFloorQuad());
			Position position = new Position(-1, -1, -1);
			if (collide.hit) {
				collide.point += Vector3.One / 2;
				Vector3 point = new Vector3(MathF.Floor(collide.point.X), 0, MathF.Floor(collide.point.Z));
				position = new Position(point);
			}
			return position;
		}

		/// Has access to render loop
		public void UpdateControl()
		{
			Position position = GetMouseTilePosition();
			Tile selected = scene.GetTile(position);
			if (selected != Tile.nullTile)
				DrawCubeWiresV(position.ToVector3(), Vector3.One, Color.ORANGE);

			if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
				if (selected.unit != null)
					selectedUnit = selected.unit; // Shouldn't matter
				else
					scene.MoveUnit(selectedUnit, position);
			}

			if (selectedUnit != null)
				scene.PushDebugText(new DebugText(selectedUnit.name, 10, 10, 21, Color.BLACK));
		}
	}
}
