using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Nodes;
using TAC.Editor;
using static Raylib_cs.Raylib;

namespace TAC.World
{
	/// <summary>
	/// Collection of tiles, walls, and objects in a convenient interface
	/// </summary>
	public class SceneTileSpace
	{
		// Array of tiles in scene
		private Tile[,,] TileMap { get; }
		// Does y level texture need re-rendering?
		// FIXME: GPU based tilemap rendering does not need this at all
		private bool[] IsLevelDirty;
		// FIXME dictionary is probably better than storing per tile?
		// Would be more logical if attached per tile?
		// Items stored per position
		private Dictionary<Position, List<Item>> TileItemMap;

		private Raylib_cs.Model[] FloorModels;
		private RenderTexture2D[] FloorTextures;

		public Position Size { get; private set; }
		public int Width { get => Size.x; } // Left right
		public int Height { get => Size.y; } // Up down
		public int Length { get => Size.z; } // Forward backward

		public ref Tile this[int x, int y, int z] => ref TileMap[x, y, z];
		public ref Tile this[Position pos] => ref this[pos.x, pos.y, pos.z];

		public SceneTileSpace(Position Size)
		{
			this.Size = Size;
			// FIXME put z coordinate in last index for cache locality
			TileMap = new Tile[Width, Height, Length];
			IsLevelDirty = new bool[Height];
			// Clear last floor
			for (int x = 0; x < Width; x++) {
				for (int z = 0; z < Length; z++) {
					TileMap[x, 0, z] = new Tile(1, 0);
				}
			}
			TileItemMap = new Dictionary<Position, List<Item>>();
			FloorModels = new Raylib_cs.Model[Height];
			FloorTextures = new RenderTexture2D[Height];
		}

		/// <summary>
		/// Generate all floor meshes based of a given lookup table
		/// </summary>
		public void GenerateFloorMeshes(List<Texture> TileLookupTable, ResourceCache cache)
		{
			for (int y = 0; y < Height; y++) GenerateFloorMesh(TileLookupTable, cache, y);
		}

		/// <summary>
		/// Regenerate and update floor texture
		/// </summary>
		public void UpdateFloorTexture(List<Texture> TileLookupTable, ResourceCache cache, int y)
		{
			//FloorTextures[y] = LoadRenderTexture(128 * Width, 128 * Length);
			BeginTextureMode(FloorTextures[y]);
			ClearBackground(Color.BLANK);
			for (int x = 0; x < Width; x++) {
				for (int z = 0; z < Length; z++) {
					int type = TileMap[x, y, z].type;
					Texture2D tex = TileLookupTable[type].texture;
					DrawTexture(tex, 128 * x, 128 * (Length - z - 1), Color.WHITE);
				}
			}
			EndTextureMode();
		}

		/// <summary>
		/// Generate a specific y level based of a given lookup table
		/// </summary>
		public void GenerateFloorMesh(List<Texture> TileLookupTable, ResourceCache cache, int y)
		{
			// Push vertices
			FloorModels[y] = LoadModelFromMesh(GenMeshPlane(1, 1, 1, 1));
			Matrix4x4 scale = Raymath.MatrixScale(Width, Height, Length);
			Matrix4x4 translate = Raymath.MatrixTranslate(Width / 2 - 0.5f, y * 2.0f, Length / 2 - 0.5f);
			FloorModels[y].transform = Raymath.MatrixMultiply(scale, translate);

			// Attach textures
			FloorTextures[y] = LoadRenderTexture(128 * Width, 128 * Length);
			BeginTextureMode(FloorTextures[y]);
			ClearBackground(Color.BLANK);
			for (int x = 0; x < Width; x++) {
				for (int z = 0; z < Length; z++) {
					int type = TileMap[x, y, z].type;
					Texture2D tex = TileLookupTable[type].texture;
					DrawTexture(tex, 128 * x, 128 * z, Color.WHITE);
				}
			}
			EndTextureMode();
			// Setting texture filters is always polite
			SetTextureWrap(FloorTextures[y].texture, TextureWrap.TEXTURE_WRAP_CLAMP);
			SetTextureFilter(FloorTextures[y].texture, TextureFilter.TEXTURE_FILTER_POINT);

			SetMaterialTexture(ref FloorModels[y], 0, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, ref FloorTextures[y].texture);
			SetMaterialShader(ref FloorModels[y], 0, ref cache.TilemapShader);
		}

		/// <summary>
		/// Update floor textures if needed
		/// </summary>
		public void UpdateFloorTextures(List<Texture> TileLookupTable, ResourceCache cache)
		{
			for (int y = 0; y < Height; y++) {
				if (IsLevelDirty[y]) {
					UpdateFloorTexture(TileLookupTable, cache, y);
					IsLevelDirty[y] = false;
				}
			}
		}

		/// <summary>
		/// Is position usable as map index
		/// </summary>
		public bool IsPositionWithinTilespace(Position pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
			   pos.x < Size.x && pos.y < Size.y && pos.z < Size.z;
		}

		public Tile GetTile(Position pos) => IsPositionWithinTilespace(pos) ? TileMap[pos.x, pos.y, pos.z] : Tile.nullTile;

		// No bounds checking because honestly if you manage to pass a negative value here it's your fault
		public void SetTile(Tile tile, Position pos) => SetTile(tile, pos.x, pos.y, pos.z);
		public void SetTile(Tile tile, int x, int y, int z)
		{
			// Whole floor needs updating. Should be unnecessary when moving to GPU rendering
			if (TileMap[x, y, z].type != tile.type) IsLevelDirty[y] = true;
			TileMap[x, y, z] = tile;
		}

		public void SetTileUnit(Unit unit, Position pos) { if (IsPositionWithinTilespace(pos)) this[pos].unit = unit; }

		// Nullable shenanigans in here, profilers beware!
		public List<Item> GetInventoryAt(Position pos) => TileItemMap.GetValueOrDefault(pos, null);

		public Raylib_cs.Model GetFloorModel(int level) => FloorModels[level];

		public JsonNode GetJsonNode()
		{
			// Create root tilespace node
			JsonObject node = new JsonObject();
			node["Size"] = new JsonArray { Size.x, Size.y, Size.z };

			node["TileMap"] = new JsonArray();
			for (int x = 0; x < Width; x++) {
				node["TileMap"].AsArray().Add(new JsonArray());
				for (int y = 0; y < Height; y++) {
					node["TileMap"][x].AsArray().Add(new JsonArray());
					for (int z = 0; z < Length; z++) {
						node["TileMap"][x][y].AsArray().Add(TileMap[x, y, z].GetJsonNode());
					}
				}
			}

			// FIXME serialize inventory dictionary

			return node;
		}
	}
}
