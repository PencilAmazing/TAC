using Raylib_cs;
using System;
using System.Numerics;
using TAC.UISystem;
using TAC.World;
using static Raylib_cs.CameraProjection;
using static Raylib_cs.KeyboardKey;
using static Raylib_cs.Raylib;

namespace TAC.Render
{
	public class CameraControl
	{
		public Camera3D camera;
		public float speed;
		private float sensitivity = 0.01f;
		private Vector2 angle = new(0.0f, MathF.PI / 6);
		private float zoom = 10.0f;

		private Unit selectedUnit;
		private Scene scene;

		public CameraControl(Scene scene, float speed = 0.5f)
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
				// Directly apply mouse delta to angle
				angle += mouse * sensitivity;
				angle.Y = (float)Math.Clamp(angle.Y, MathF.PI / 2 + 0.1, MathF.PI - 0.1);
			}
			camera.position = camera.target - Raymath.Vector3RotateByQuaternion(Vector3.UnitZ * zoom, Raymath.QuaternionFromEuler(angle.Y, angle.X, 0));
		}

		private Position GetMouseTilePosition()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera);
			// replace this with quad collision instead
			RayCollision collide = GetRayCollisionModel(ray, scene.GetFloorQuad());
			if (collide.hit) {
				collide.point += Vector3.One / 2;
				// Floor
				return new Position((int)(collide.point.X), 0, (int)(collide.point.Z));
			}
			return Position.Negative;
		}

		private Vector3 CollideMouseWithPlane()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera);
			// replace this with quad collision instead
			RayCollision collide = GetRayCollisionModel(ray, scene.GetFloorQuad());
			if (collide.hit) {
				//collide.point += Vector3.One / 2;
				return new Vector3(collide.point.X, 0, collide.point.Z);
			}
			return -Vector3.One;
		}

		// Has access to render loop
		public void UpdateGameControl()
		{
			Position position = GetMouseTilePosition();
			Tile selected = scene.GetTile(position);
			if (selected != Tile.nullTile)
				DrawCubeWiresV(position.ToVector3(), Vector3.One, Color.ORANGE);

			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
				if (selected.unit != null)
					selectedUnit = selected.unit; // Shouldn't matter
				else
					scene.MoveUnit(selectedUnit, position);
			}

			if (selectedUnit != null)
				scene.PushDebugText(new DebugText(selectedUnit.name, 10, 10, 21, Color.BLACK));
		}

		public void UpdateEditControl()
		{
			Vector3 point = CollideMouseWithPlane();
			Position position = new Position((int)(point.X + 0.5f), 0, (int)(point.Z + 0.5f));

			Tile selected = scene.GetTile(position);
			if (selected == Tile.nullTile) return;
			DrawCubeWiresV(position.ToVector3(), Vector3.One, Color.ORANGE);

			Vector3 diff = point - position.ToVector3();
			Wall wall = Wall.North;
			if (MathF.Abs(diff.X) > MathF.Abs(diff.Z)) {
				wall = Wall.West;
				if (diff.X > 0)
					position += new Position(1, 0, 0);
				diff = -Vector3.UnitX;
			} else {
				if (diff.Z > 0)
					position += new Position(0, 0, 1);
				diff = -Vector3.UnitZ;
			}

			DrawSphereWires(position.ToVector3() + diff / 2, 0.1f, 4, 4, Color.BLUE);

			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
				scene.ToggleBrush(position, wall, 1);
				scene.ToggleWall(position, wall);
			}
			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_RIGHT)) {
				scene.ToggleWall(position, (Wall)((byte)wall<<2));
			}
		}

	}
}
