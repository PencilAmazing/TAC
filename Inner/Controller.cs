﻿using Raylib_cs;
using System;
using System.Numerics;
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
		private GameSelection mode;
		public CameraControl camera;
		public Unit selectedUnit { get; private set; }

		public PlayerController(Scene scene, float speed = 0.5f)
		{
			this.scene = scene;
			camera = new CameraControl(scene, speed);
			mode = GameSelection.SelectUnit;
		}

		// Has access to render loop
		public void UpdateGameControl()
		{
			Position selectedPosition = GetMouseTilePosition();
			Tile selected = scene.GetTile(selectedPosition);
			if (selected != Tile.nullTile)
				DrawCubeWiresV(selectedPosition.ToVector3(), Vector3.One, Color.ORANGE);

			if (mode == GameSelection.SelectUnit && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
				if (selected.unit != null)
					selectedUnit = selected.unit; // Shouldn't be called often
				else
					scene.PushActionMoveUnit(selectedUnit, selectedPosition);
			} else if (mode == GameSelection.SelectUnit && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_RIGHT)) {
				scene.PushActionTurnUnit(selectedUnit, selectedPosition);
			} else if (mode == GameSelection.SelectTarget) {
				// Potential target
				Position potential = GetMouseTilePosition();
				if (scene.IsTileWithinBounds(potential) && UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_LEFT)) {
					//scene.debugPath = scene.GetSupercoverLine(selectedUnit.position, potential);
					this.mode = GameSelection.SelectUnit;
					scene.PushActionSelectTarget(selectedUnit, selectedUnit.inventory[0], potential);
				}
			}
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
				// Also toggles wall, BTW
				//Console.Write("From " + Convert.ToString(scene.GetTile(position).walls, 2).PadLeft(8, '0'));
				scene.ToggleBrush(position, wall, 1);
				//Console.WriteLine("To " + Convert.ToString(scene.GetTile(position).walls, 2).PadLeft(8, '0'));
			}
			if (UI.GetMouseButtonPress(MouseButton.MOUSE_BUTTON_RIGHT)) {
				/** FIXME This thing has caused me so much trouble
					Can sometimes just delete all wall bits i think? **/
				//Console.Write("From " + Convert.ToString(scene.GetTile(position).walls, 2).PadLeft(8, '0'));
				//scene.ToggleWall(position, (Wall)((byte)wall << 2));
				//Console.WriteLine("To " + Convert.ToString(scene.GetTile(position).walls, 2).PadLeft(8, '0'));
			}
		}

		/// <summary>
		/// Create an action tracing a supercover line
		/// from selected unit to another point
		/// </summary>
		public void StartSelectingTarget()
		{
			this.mode = GameSelection.SelectTarget;
		}

		private Position GetMouseTilePosition()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera.camera);
			unsafe {
				// replace this with GetRayCollisionQuad instead
				RayCollision collide = GetRayCollisionMesh(ray, scene.GetFloorQuad().meshes[0], scene.GetFloorQuad().transform);
				if (collide.hit) {
					collide.point += Vector3.One / 2;
					// Floor
					Position mousePos = new Position((int)(collide.point.X), 0, (int)(collide.point.Z));
					//scene.PushDebugText(new DebugText(mousePos.ToString(), 50, 50, 12, Color.BLACK));
					return mousePos;
				}
			}
			return Position.Negative;
		}

		private Vector3 CollideMouseWithPlane()
		{
			Ray ray = GetMouseRay(GetMousePosition(), camera.camera);
			unsafe {
				// replace this with GetRayCollisionQuad instead
				RayCollision collide = GetRayCollisionMesh(ray, scene.GetFloorQuad().meshes[0], scene.GetFloorQuad().transform);
				if (collide.hit) {
					//collide.point += Vector3.One / 2;
					return new Vector3(collide.point.X, 0, collide.point.Z);
				}
			}
			return -Vector3.One;
		}
	}
}
