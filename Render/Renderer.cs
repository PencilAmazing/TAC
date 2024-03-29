﻿using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using TAC.Editor;
using TAC.World;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace TAC.Render
{
	// Consider making static?
	public class Renderer
	{
		// Shame
		//public void DrawUnits(Camera3D camera, List<Unit> units, ResourceCache cache)
		//{
		//	if (units.Count == 0) return;
		//  // For animating billboards
		//	SetShaderValueV(cache.BillboardShader, cache.BillboardTexCoordShiftLoc,
		//		new float[] { 0, 1 }, // No scrolling, take whole texture
		//		ShaderUniformDataType.SHADER_UNIFORM_VEC2, 1);
		//	BeginShaderMode(cache.BillboardShader);
		//	foreach (Unit unit in units) {
		//		Texture2D tex = unit.Type.texture.texture;
		//		Vector3 position = unit.position.ToVector3() + Vector3.UnitY;
		//		// TODO: move this to GPU
		//		// Vector from unit to camera
		//		Vector3 dir = camera.position - unit.position.ToVector3();
		//		// X coord is right, Z is forward
		//		// Can we avoid floating point math here?
		//		double angle = Math.Atan2(dir.X, dir.Z) / (2 * Math.PI);
		//		// Offset tile by unit direction to get display angle
		//		int unitForward = (int)unit.direction;
		//		// No clue what's going on here
		//		// FIXME added 8 since offset can become negative sometimes. Make something better ffs
		//		int offset = ((int)(8 * angle + 8.5) - unitForward + 8) % 8;
		//		Rectangle rec = new Rectangle(128 * offset, 0, 128, 128);
		//		DrawBillboardPro(camera, tex, rec, position, Vector3.UnitY, Vector2.One * 2, Vector2.Zero, 0.0f, Color.WHITE);
		//	}
		//	EndShaderMode();
		//}

		/// <summary>
		/// Render a unit based on it's values and template
		/// </summary>
		public void DrawUnit(Camera3D camera, Unit unit, ResourceCache cache)
		{
			/* Plan in here:
			   Collect model and load material
			   Create transformation matrix from unit data. DO NOT bother with model built in transform
			   Any shader inside the model would choose correct billboard orientation
			*/

			UnitTemplate template = unit.Template;
			Vector3 unitPosition = unit.position.ToVector3();

			if (template.Type == UnitTemplate.TemplateType.Skeletal) {
				DrawModelEx(template.Model.model, unitPosition, Vector3.UnitY, (int)unit.direction * 45.0f, Vector3.One, Color.WHITE);
			} else {
				// Vector from unit to camera
				Vector3 dir = camera.position - unitPosition;
				// X coord is right, Z is forward
				// Can we avoid floating point math here?
				double angle = Math.Atan2(dir.X, dir.Z) / (2 * Math.PI);
				// Offset tile by unit direction to get display angle
				int unitForward = (int)unit.direction;
				// No clue what's going on here
				// FIXME added 8 since offset can become negative sometimes. Make something better ffs
				int offset = ((int)(8 * angle + 8.5) - unitForward + 8) % 8;
				SetShaderValueV(cache.BillboardShader, cache.BillboardTexCoordShiftLoc,
								// integer divisions were a mistake
								new float[] { offset / 8.0f, 1.0f / 8.0f },
								ShaderUniformDataType.SHADER_UNIFORM_VEC2, 1);
				SetMaterialTexture(ref cache.BillboardMaterial, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, template.BillboardTexture.texture);
				DrawMesh(cache.BillboardMesh, cache.BillboardMaterial, MatrixTranslate(unitPosition.X, unitPosition.Y, unitPosition.Z));
			}
		}

		public void DrawEffect(Camera3D camera, ParticleEffect effect, ResourceCache cache)
		{
			Texture2D misctex = effect.sprite.texture.texture;
			int stage = effect.GetStage();
			Rectangle rect = effect.sprite.GetRectangle(stage);

			SetShaderValueV(cache.BillboardShader, cache.BillboardTexCoordShiftLoc,
							new float[] { rect.x / misctex.width, rect.width / misctex.width },
							ShaderUniformDataType.SHADER_UNIFORM_VEC2, 1);

			Rlgl.rlDisableBackfaceCulling();
			SetMaterialTexture(ref cache.crossMaterial, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, misctex);
			DrawMesh(cache.cross, cache.crossMaterial,
					 MatrixTranslate(effect.position.X, effect.position.Y, effect.position.Z) *
					 MatrixRotateXYZ(effect.rotate) *
					 MatrixScale(effect.scale.X, effect.scale.Y, effect.scale.Z));
			Rlgl.rlEnableBackfaceCulling();
		}

		public void DrawUnitDebug(Camera3D camera, List<Unit> units, ResourceCache cache)
		{
			if (units.Count == 0) return;

			foreach (Unit unit in units) {
				Vector3 dir = Unit.VectorDirections[(int)unit.direction];
				Vector3 perp = Unit.VectorDirections[(int)(unit.direction + 2) % 8];
				Vector3 position = unit.position.ToVector3() + Vector3.UnitY;
				// Unit direction
				DrawLine3D(position, position + dir * 2, Color.GREEN);
				// Perpendicular direction
				DrawLine3D(position, position + perp * 2, Color.RED);
			}
		}

		/// <summary>
		/// Draw tiles on ground
		/// </summary>
		public void DrawFloor(ResourceCache cache, Raylib_cs.Model FloorModel, List<Texture> TileLookupTable)
		{
			// Offset ground in transform matrix to make other rendering easier
			// FIXME Texture arrays do not exist in raylib bro wtf
			// Offset rendering by epsilion
			Rlgl.rlDisableBackfaceCulling();
			DrawModel(FloorModel, Vector3.UnitY * 0.001f, 1, Color.WHITE);
			Rlgl.rlEnableBackfaceCulling();
		}

		/// <summary>
		/// angle multiplier modulo 4
		/// rotate if drawing east/west wall
		/// </summary>
		public void DrawWall(Camera3D camera, Vector3 center, bool rotate, bool flip, Brush tex, ResourceCache cache)
		{
			//Matrix4x4 transform = MatrixScale(1, 1, 0.1f); // Scale box to wall shape
			//transform = MatrixTranslate(0.0f, 0.5f, -0.5f) * transform; // Offset to edge of tile
			//if (rotate) transform = MatrixRotateY(MathF.PI / 2) * transform; // Rotate if west wall
			//if (flip) transform = MatrixRotateY(MathF.PI) * transform; // rotate 180 to flip texture

			Matrix4x4 transform = rotate ? cache.WallTransformWest : cache.WallTransformNorth;
			if (flip) transform = transform * MatrixRotateY(MathF.PI); // rotate 180 to flip texture

			transform = MatrixTranslate(center.X, center.Y * 2, center.Z) * transform;

			unsafe {
				// Assign textures to locations
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 0] = cache.toploc;
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 1] = cache.leftloc;
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 2] = cache.frontloc;
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 3] = cache.bottomloc;
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 4] = cache.rightloc;
				cache.wallMaterial.shader.locs[(int)ShaderLocationIndex.SHADER_LOC_MAP_DIFFUSE + 5] = cache.backloc;

				// Assign textures to shader
				cache.wallMaterial.maps[0].texture = tex.top.texture;
				cache.wallMaterial.maps[1].texture = tex.left.texture;
				cache.wallMaterial.maps[2].texture = tex.front.texture;
				cache.wallMaterial.maps[3].texture = tex.bottom.texture;
				cache.wallMaterial.maps[4].texture = tex.right.texture;
				cache.wallMaterial.maps[5].texture = tex.back.texture;
			}
			// Draw mesh internally binds texture units
			DrawMesh(cache.cube, cache.wallMaterial, transform); // Magic!

			/*BoundingBox box = GetMeshBoundingBox(cache.cube);
			box.min = Vector3Transform(box.min, transform);
			box.max = Vector3Transform(box.max, transform);
			DrawBoundingBox(box, Color.RED);*/
		}

		public void DrawThing(Thing thing, Vector3 position)
		{
			DrawModel(thing.model.model, position, 1.0f, Color.WHITE);
		}

		public void DrawSkybox(Camera3D camera, ResourceCache cache)
		{
			// We are inside the cube, we need to disable backface culling!
			Rlgl.rlDisableBackfaceCulling();
			Rlgl.rlDisableDepthMask();
			DrawMesh(cache.cube, cache.SkyboxMaterial, Matrix4x4.Identity);
			Rlgl.rlEnableBackfaceCulling();
			Rlgl.rlEnableDepthMask();
		}

		public void DrawDebugPath(Position[] positions)
		{
			float freq = MathF.PI * positions.Length / 2;
			for (int i = 0; i < positions.Length; i++) {
				// Thank god for HSV
				float color = i * 360 / positions.Length;
				DrawCubeV(positions[i].ToVector3(), Vector3.One / 2, ColorFromHSV(color, 0.5f, 0.5f));
			}
		}

		public void DrawDebugPositions(Position[] positions)
		{
			float freq = MathF.PI * positions.Length / 2;
			for (int i = 0; i < positions.Length; i++) {
				// Thank god for HSV
				float color = i * 360.0f / positions.Length;
				DrawCubeV(positions[i].ToVector3() + Vector3.UnitY * 0.1f, Vector3.One / 3, ColorFromHSV(1.0f, 0.0f, color));
			}
		}

		public void DrawDebugLine(Vector3 from, Vector3 to, Color color) => Raylib.DrawLine3D(from, to, color);
	}
}
