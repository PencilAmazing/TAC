using Raylib_cs;
using System.Collections.Generic;
using System;
using System.Numerics;
using static Raylib_cs.Raylib;
using TAC.World;
using static Raylib_cs.Raymath;

namespace TAC.Render
{
	struct DebugText
	{
		public string text;
		public int posx;
		public int posy;
		public int fontSize;
		public Color color;
		public DebugText(string text, int posx, int posy, int fontSize, Color color)
		{
			this.text = text;
			this.posx = posx;
			this.posy = posy;
			this.fontSize = fontSize;
			this.color = color;
		}
	}

	struct WallTexture
	{
		public int top;
		public int front;
		public int bottom;
		public int back;
		public int right;
		public int left;

		public WallTexture()
		{
			top = 1;
			front = 2;
			bottom = 3;
			back = 4;
			right = 5;
			left = 6;
		}

		public WallTexture(int top = 6, int front = 1, int bottom = 2, int back = 3, int right = 4, int left = 5)
		{
			this.top = top;
			this.front = front;
			this.bottom = bottom;
			this.back = back;
			this.right = right;
			this.left = left;
		}
	}

	// Consider making static?
	class Renderer
	{
		public Renderer() { }

		public void DrawUnits(Camera3D camera, List<Unit> units, ResourceCache cache)
		{
			if (units.Count == 0) return;

			BeginShaderMode(cache.BillboardShader);
			foreach (Unit unit in units) {
				Texture2D tex = cache.units[unit.type];
				Vector3 position = unit.position.ToVector3() + Vector3.UnitY;
				// TODO: move this to GPU
				// Vector from unit to camera
				Vector3 dir = camera.position - unit.position.ToVector3();
				// X coord is right, Z is forward
				// Can we avoid floating point math here?
				double angle = Math.Atan2(dir.X, dir.Z) / (2 * Math.PI);
				// Offset tile by unit direction to get display angle
				int unitForward = (int)unit.direction;
				// No clue what's going on here
				int offset = ((int)(8 * angle + 8.5) - unitForward) % 8;
				Rectangle rec = new Rectangle(128 * offset, 0, 128, 128);
				DrawBillboardPro(camera, tex, rec, position, Vector3.UnitY, Vector2.One * 2, Vector2.Zero, 0.0f, Color.WHITE);
			}
			EndShaderMode();
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

		public void DrawFloor(Camera3D camera, Floor floor, ResourceCache cache)
		{
			// Offset ground in transform matrix to make other rendering easier
			DrawModel(floor.GetQuad(), Vector3.Zero, 1, Color.WHITE);
		}

		/// <summary>
		/// angle multiplier modulo 4
		/// </summary>
		public void DrawWall(Camera3D camera, Vector3 center, bool rotate, bool flip, WallTexture tex, ResourceCache cache)
		{
			// center tile
			Matrix4x4 translate = MatrixTranslate(center.X, center.Y, center.Z);
			if (rotate) translate *= MatrixRotateY(MathF.PI / 2); // Rotate if west wall
			translate *= MatrixTranslate(0.0f, 0.5f, -0.5f); // Offset to edge of tile
			if(flip) translate *= MatrixRotateY(MathF.PI); // rotate 180 to flip texture
			translate *= MatrixScale(1, 1, 0.1f);// Scale box to wall shape

			// Upload textures to GPU
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)0, cache.tiles[tex.top]);
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)1, cache.tiles[tex.left]);
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)2, cache.tiles[tex.front]);
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)3, cache.tiles[tex.bottom]);
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)4, cache.tiles[tex.right]);
			SetMaterialTexture(ref cache.wallMaterial, (MaterialMapIndex)5, cache.tiles[tex.back]);

			// Bind textures to shader
			SetShaderValueTexture(cache.wallMaterial.shader, cache.toploc, cache.tiles[tex.top]);
			SetShaderValueTexture(cache.wallMaterial.shader, cache.leftloc, cache.tiles[tex.left]);
			SetShaderValueTexture(cache.wallMaterial.shader, cache.frontloc, cache.tiles[tex.front]);
			SetShaderValueTexture(cache.wallMaterial.shader, cache.bottomloc, cache.tiles[tex.bottom]);
			SetShaderValueTexture(cache.wallMaterial.shader, cache.rightloc, cache.tiles[tex.right]);
			SetShaderValueTexture(cache.wallMaterial.shader, cache.backloc, cache.tiles[tex.back]);

			DrawMesh(cache.cube, cache.wallMaterial, translate); // Magic!
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
	}
}
