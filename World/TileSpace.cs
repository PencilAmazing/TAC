using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
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
		// FIXME dictionary is probably better than storing per tile?
		// Would be more logical if attached per tile?
		// Items stored per position
		private Dictionary<Position, List<Item>> TileItemMap;

		private Model[] FloorModels;
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
			TileItemMap = new Dictionary<Position, List<Item>>();
			FloorModels = new Model[Height];
			FloorTextures = new RenderTexture2D[Height];
		}

		public void GenerateFloorMeshes(List<Texture> TileLookupTable, ResourceCache cache)
		{
			for (int y = 0; y < Height; y++) {
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
						Texture2D tex = TileLookupTable[type].tex;
						DrawTexture(tex, 128 * x, 128 * z, Color.WHITE);
					}
				}
				EndTextureMode();

				SetMaterialTexture(ref FloorModels[y], 0, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, ref FloorTextures[y].texture);
				SetMaterialShader(ref FloorModels[y], 0, ref cache.TilemapShader);
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

		public void SetTile(Tile tile, Position pos) => SetTile(tile, pos.x, pos.y, pos.z);
		public void SetTile(Tile tile, int x, int y, int z) => TileMap[x, y, z] = tile;


		public void SetTileUnit(Unit unit, Position pos) { if (IsPositionWithinTilespace(pos)) this[pos].unit = unit; }

		// Nullable shenanigans in here, profilers beware!
		public List<Item> GetInventoryAt(Position pos) => TileItemMap.GetValueOrDefault(pos, null);

		public Model GetFloorModel(int level) => FloorModels[level];
	}
}
