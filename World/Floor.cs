using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Color;
using static Raylib_cs.MaterialMapIndex;
using static Raylib_cs.Raylib;
using TAC.Editor;
using System.Collections.Generic;

namespace TAC.World
{
	/// <summary>
	/// Representation of floor tiles and static objects
	/// </summary>
	public class Floor
	{
		public int length { get; }
		public int width { get; }
		public Tile[,] map { get; }

		/// <summary>
		/// Used to scale floor quad
		/// </summary>
		public Vector3 size { get; }
		//private Rectangle quadRect;
		private RenderTexture2D texture;
		private Model quad;

		public Floor(int length, int width)
		{
			this.length = length;
			this.width = width;
			map = new Tile[length, width];

			texture = LoadRenderTexture(128 * length, 128 * width);
			size = new Vector3(length, 8, width);

			quad = LoadModelFromMesh(GenMeshPlane(1, 1, 1, 1));
			Matrix4x4 scale = Raymath.MatrixScale(size.X, size.Y, size.Z);
			Matrix4x4 translate = Raymath.MatrixTranslate(size.X / 2 - 0.5f, 0, size.Z / 2 - 0.5f);
			quad.transform = Raymath.MatrixMultiply(scale, translate);
		}

		~Floor()
		{
			UnloadModel(quad);
			UnloadRenderTexture(texture);
		}

		// I hate this
		public ref Tile this[int x, int z]
		{
			get { return ref map[x, z]; }
		}

		public void CreateTexture(List<Texture> tileLookupTable)
		{
			BeginTextureMode(texture);
			ClearBackground(WHITE);
			for (int i = 0; i < length; i++) {
				for (int j = 0; j < width; j++) {
					Texture2D tex = tileLookupTable[map[i, j].type].tex;
					DrawTexture(tex, 128 * i, 128 * j, WHITE);
				}
			}
			EndTextureMode();

			SetMaterialTexture(ref quad, 0, MATERIAL_MAP_DIFFUSE, ref texture.texture);
		}

		public Model GetQuad()
		{
			return quad;
		}

		public Tile GetTile(Position pos) => GetTile(pos.x, pos.z);

		public Tile GetTile(int x, int z)
		{
			if (x < 0 || z < 0 || x > map.GetUpperBound(0) || z > map.GetUpperBound(1))
				return Tile.nullTile;
			return map[x, z];
		}

		public void SetTile(Position pos, Tile tile)
		{
			if (pos.x < 0 || pos.z < 0 || pos.x > map.GetUpperBound(0) || pos.z > map.GetUpperBound(1)) return;
			map[pos.x, pos.z] = tile;
		}

		public void SetTileUnit(Position pos, Unit unit)
		{
			if (pos.x < 0 || pos.z < 0 || pos.x > map.GetUpperBound(0) || pos.z > map.GetUpperBound(1)) return;
			map[pos.x, pos.z].unit = unit;
		}
		public void SetTileType(Position pos, int type)
		{
			if (pos.x < 0 || pos.z < 0 || pos.x >= map.GetUpperBound(0) || pos.z >= map.GetUpperBound(1)) return;
			map[pos.x, pos.z].type = type;
		}
	}
}
