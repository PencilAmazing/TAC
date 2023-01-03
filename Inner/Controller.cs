using Raylib_cs;
using System;
using System.Numerics;
using TAC.Inner.ControllerStates;
using TAC.Render;
using TAC.UISystem;
using TAC.World;
using static Raylib_cs.Raylib;

namespace TAC.Inner
{
	public class PlayerController
	{
		/// <summary>
		/// Game selection mode
		/// </summary>
		public enum GameSelection
		{
			SelectUnit, // Default game mode, select units to view
			SelectTarget,
			WaitAction
		}

		private Scene scene;
		public CameraControl camera;

		// Boy I sure do love maintaining state
		public ControlEditState EditState;
		public ControlGameState GameState;

		// Properties suck
		public Unit SelectedUnit { get { return GameState.SelectedUnit; } set { GameState.SelectedUnit = value; } }
		public GameSelection SelectionMode { get { return GameState.Mode; } }

		public PlayerController(Scene scene, float speed = 0.5f)
		{
			this.scene = scene;
			camera = new CameraControl(scene, speed);
			GameState = new ControlGameState();
			EditState = new ControlEditState();
		}

		// Has access to render loop
		public void UpdateGameControl()
		{
			Position selectedPosition = GetMouseTilePosition();
			Tile selected = scene.GetTile(selectedPosition);
			if (selected != Tile.nullTile)
				DrawCubeWiresV(selectedPosition.ToVector3() + Vector3.UnitY * selectedPosition.y, Vector3.One, Color.ORANGE);

			if (GameState.Mode == GameSelection.SelectUnit && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
				if (selected.unit != null && GameState.SelectedTeam.HasUnit(selected.unit))
					GameState.SelectedUnit = selected.unit; // Select unit only if in team
				else
					scene.PushActionMoveUnit(GameState.SelectedUnit, selectedPosition);
			} else if (GameState.Mode == GameSelection.SelectUnit && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_RIGHT)) {
				scene.PushActionTurnUnit(GameState.SelectedUnit, selectedPosition);
			} else if (GameState.Mode == GameSelection.SelectTarget) {
				// Potential target
				Position potential = GetMouseTilePosition();
				if (scene.IsTileWithinBounds(potential) && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
					GameState.Mode = GameSelection.SelectUnit;
					scene.PushActionSelectTarget(GameState.SelectedUnit, GameState.SelectedUnit.inventory[0], potential);
				}
			}

			if (UI.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL) && UI.IsKeyDown(KeyboardKey.KEY_ENTER)) {
				scene.EndTurn();
			}
		}

		public void UpdateEditControl()
		{
			Vector3 point = CollideMouseWithPlane();
			Position position = new Position(point);

			Tile selected = scene.GetTile(position);
			if (selected == Tile.nullTile) return;
			DrawCubeWiresV(position.ToVector3() + Vector3.UnitY * position.y, Vector3.One, Color.ORANGE);

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

			DrawSphereWires(position.ToVector3() + diff / 2 + Vector3.UnitY * position.y, 0.1f, 4, 4, Color.BLUE);

			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
				// Also toggles wall, BTW
				if (EditState.selectedBrush != null) {
					if (EditState.FlipBrush)
						scene.ToggleBrush(position, wall | (Wall)((byte)wall << 2), EditState.selectedBrush);
					else
						scene.ToggleBrush(position, wall, EditState.selectedBrush);
				}
			}
			// Much better
			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_RIGHT)) EditState.FlipBrush = !EditState.FlipBrush;
		}

		/// <summary>
		/// Create an action tracing a supercover line
		/// from selected unit to another point
		/// </summary>
		public void StartSelectingTarget()
		{
			GameState.Mode = GameSelection.SelectTarget;
		}

		public Position GetMouseTilePosition()
		{
			Vector3 worldHit = CollideMouseWithPlane();

			if (worldHit == -Vector3.One) return Position.Negative;

			return new Position(worldHit);
		}

		private Vector3 CollideMouseWithPlane()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera.camera);
			bool everHit = false;
			RayCollision collide;
			Vector3 outHit = -Vector3.One;

			for (int i = 0; i < scene.Size.y; i++) {
				unsafe {
					// TODO replace this with GetRayCollisionQuad instead
					collide = GetRayCollisionMesh(ray, scene.GetFloorModel(i).meshes[0], scene.GetFloorModel(i).transform);
				}

				if (collide.hit && (!everHit ||
					Vector3.DistanceSquared(ray.position, collide.point) < Vector3.DistanceSquared(ray.position, outHit))) {
					if (scene.GetTile(new Position(collide.point)).type == 0) continue; // If we hit clear floor skip
					outHit = collide.point;
					everHit = true;
				}
			}
			return outHit;
		}

		public void SetSelectedBrush(Brush brush)
		{
			if (Brush.IsBrushValid(brush)) EditState.selectedBrush = brush;
		}
	}
}
