using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using TAC.Render;
using TAC.World;
using static Raylib_cs.MaterialMapIndex;
using static Raylib_cs.Raylib;
using static Raylib_cs.ShaderUniformDataType;
using static Raylib_cs.Raymath;

namespace TAC.Editor
{
	// Maps textures loaded in memory to tile IDs
	public class ResourceCache
	{
		private string AssetTilePrefix = "assets/tile/";
		private string AssetUnitPrefix = "assets/unit/";
		private string AssetShaderPrefix = "assets/shader/";
		private string AssetScenePrefix = "assets/scene/";
		private string AssetSaveDirectory = "assets/save";

		// Texture locations in the wall shader.
		public int toploc;
		public int bottomloc;
		public int leftloc;
		public int rightloc;
		public int frontloc;
		public int backloc;

		public enum TextureType
		{
			TEX_TILE,
			TEX_UNIT,
			TEX_ITEM,
			TEX_EFFECT,
			TEX_MISC
		}

		// Might be inefficient, whatever
		// Here's an idea: merge them all into one mega list
		public List<Texture2D> tiles { get; }
		public List<Texture2D> units { get; }
		public List<Texture2D> items { get; }
		public List<Texture2D> misc { get; }

		public List<Sprite> sprites { get; }
		public List<Brush> brushes { get; }
		public List<Model> things { get; }

		// Discard transparent pixels and shift texture
		// Assumes that texcoords are either 0 or 1 only
		public Shader BillboardShader;
		public int BillboardTexCoordShiftLoc;

		public Shader SkyboxShader;
		public Shader WallShader;

		// General purpose cube mesh
		public Mesh cube;
		public Material wallMaterial;

		// Cross mesh for particles
		public Mesh cross;
		public Material crossMaterial;

		/// <summary>
		/// UNTRANSLATED transforms <br></br>
		/// do transform = MatrixTranslate(center.X, center.Y, center.Z) * WallTransform;
		/// </summary>
		public Matrix4x4 WallTransformNorth, WallTransformWest;

		public Material SkyboxMaterial;
		public Texture2D SkyboxCubemap;

		public ResourceCache()
		{
			tiles = new List<Texture2D>();
			units = new List<Texture2D>();
			items = new List<Texture2D>();
			misc = new List<Texture2D>();
			brushes = new List<Brush>();
			brushes.Add(null); // reserve 0 slot
		}

		~ResourceCache()
		{
			foreach (Texture2D tex in tiles)
				UnloadTexture(tex);
			foreach (Texture2D tex in units)
				UnloadTexture(tex);
			foreach (Texture2D tex in items)
				UnloadTexture(tex);

			UnloadShader(BillboardShader);
			UnloadShader(SkyboxShader);

			UnloadMesh(ref cube);
			UnloadMaterial(wallMaterial);
			UnloadMaterial(SkyboxMaterial);
			UnloadTexture(SkyboxCubemap);

			foreach (Model model in things)
				UnloadModel(model);

		}

		public int CreateBrush(Brush brush)
		{
			brushes.Add(brush);
			return brushes.Count - 1;
		}


		public int LoadTexture(string ImagePath, TextureType type)
		{
			Texture2D tex = Raylib.LoadTexture(ImagePath);
			SetTextureFilter(tex, TextureFilter.TEXTURE_FILTER_POINT);
			SetTextureWrap(tex, TextureWrap.TEXTURE_WRAP_CLAMP);
			switch (type) {
				case TextureType.TEX_TILE:
					tiles.Add(tex);
					// ID used to fetch tex later
					return tiles.Count - 1;
				case TextureType.TEX_UNIT:
					units.Add(tex);
					return units.Count - 1;
				case TextureType.TEX_ITEM:
					items.Add(tex);
					return items.Count - 1;
				case TextureType.TEX_MISC:
					misc.Add(tex);
					return misc.Count - 1;
				default:
					break;
			}
			return -1;
		}

		private void LoadTiles()
		{
			LoadTexture(AssetTilePrefix + "OBKMTB90.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OBASEM37.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OBOOKA03.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OBRCKL02.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OBRCKQ12.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OBRCKQ44.png", TextureType.TEX_TILE);
			LoadTexture(AssetTilePrefix + "OCHRMA14.png", TextureType.TEX_TILE);
		}

		private void LoadUnits()
		{
			LoadTexture(AssetUnitPrefix + "mech.png", TextureType.TEX_UNIT);
		}

		private void LoadSprites()
		{
			LoadTexture(AssetScenePrefix + "sprite/explosion_11.png", TextureType.TEX_MISC);
			LoadTexture(AssetScenePrefix + "sprite/ProjectileArranged.png", TextureType.TEX_MISC);
		}

		private void LoadShaders()
		{
			BillboardShader = LoadShader(null, AssetShaderPrefix + "billboard.frag");
			BillboardTexCoordShiftLoc = GetShaderLocation(BillboardShader, "texCoordShift");

			SkyboxShader = LoadShader(AssetShaderPrefix + "skybox.vs", AssetShaderPrefix + "skybox.fs");
			SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "environmentMap"), (int)MATERIAL_MAP_CUBEMAP, SHADER_UNIFORM_INT);
			SetShaderValue(SkyboxShader, GetShaderLocation(SkyboxShader, "vflipped"), 1, SHADER_UNIFORM_INT);

			WallShader = LoadShader(AssetShaderPrefix + "wall.vert", AssetShaderPrefix + "wall.frag");
			rightloc = GetShaderLocation(WallShader, "right");
			leftloc = GetShaderLocation(WallShader, "left");
			toploc = GetShaderLocation(WallShader, "top");
			bottomloc = GetShaderLocation(WallShader, "bottom");
			backloc = GetShaderLocation(WallShader, "back");
			frontloc = GetShaderLocation(WallShader, "front");
		}

		unsafe static void AllocateMeshData(Mesh* mesh, int vertexCount, int triangleCount)
		{
			mesh->vertexCount = vertexCount;
			mesh->triangleCount = triangleCount;
			unsafe {
				mesh->vertices = (float*)MemAlloc(mesh->vertexCount * 3 * sizeof(float));
				mesh->texcoords = (float*)MemAlloc(mesh->vertexCount * 2 * sizeof(float));
				mesh->normals = (float*)MemAlloc(mesh->vertexCount * 3 * sizeof(float));
				mesh->indices = (ushort*)MemAlloc(mesh->triangleCount * 3 * sizeof(ushort));
			}
		}

		// Interally generated meshes only please
		// And other frequently used matrices i guess too
		private void GenerateUploadMeshes()
		{
			// GenMesh uploads data to GPU
			cube = GenMeshCube(1, 1, 1);
			SkyboxMaterial = LoadMaterialDefault();
			SkyboxMaterial.shader = SkyboxShader;

			// Front faces are CCW although disable culling when drawing either way
			float[] vertices = new float[] {
					// XZ plane
					-0.5f, 0, -0.5f,
					-0.5f, 0, 0.5f,
					0.5f, 0, -0.5f,
					0.5f, 0, 0.5f ,
					// XY plane
					-0.5f, -0.5f, 0,
					-0.5f, 0.5f, 0,
					0.5f, -0.5f, 0,
					0.5f, 0.5f, 0
				};

			float[] normals = new float[] {
					// Up
					0, 1.0f, 0,
					0, 1.0f, 0,
					0, 1.0f, 0,
					0, 1.0f, 0,
					// Forward
					1.0f, 0, 0,
					1.0f, 0, 0,
					1.0f, 0, 0,
					1.0f, 0, 0
				};

			float[] texcoords = new float[8 * 2];
			for (int x = 0; x <= 1; x++)
				for (int z = 0; z <= 1; z++) {
					texcoords[z * 2 + x * 4] = x;
					texcoords[z * 2 + x * 4 + 1] = z;
				}

			for (int x = 0; x <= 1; x++)
				for (int y = 0; y <= 1; y++) {
					texcoords[8 + y * 2 + x * 4] = x;
					texcoords[8 + y * 2 + x * 4 + 1] = y;
				}

			// Two faces, two triangles each, three vertices each
			ushort[] indices = new ushort[] {0, 2, 1, 1, 2, 3,
											 4, 6, 5, 5, 6, 7};

			cross = new Mesh();
			unsafe {
				fixed (Mesh* mesh = &cross) {
					AllocateMeshData(mesh, vertices.Length / 3, 2 * 2);
				}
				vertices.CopyTo(new Span<float>(cross.vertices, vertices.Length));

				normals.CopyTo(new Span<float>(cross.normals, normals.Length));
				texcoords.CopyTo(new Span<float>(cross.texcoords, texcoords.Length));
				indices.CopyTo(new Span<ushort>(cross.indices, indices.Length));
			}
			UploadMesh(ref cross, false);

			crossMaterial = LoadMaterialDefault();
			crossMaterial.shader = BillboardShader;

			Image SkyboxImage = LoadImage(AssetScenePrefix + "skybox/skybox.png");
			SkyboxCubemap = LoadTextureCubemap(SkyboxImage, CubemapLayout.CUBEMAP_LAYOUT_AUTO_DETECT);
			UnloadImage(SkyboxImage);
			SetMaterialTexture(ref SkyboxMaterial, MATERIAL_MAP_CUBEMAP, SkyboxCubemap);

			wallMaterial = LoadMaterialDefault();
			wallMaterial.shader = WallShader;
			//SetMaterialTexture(ref wallMaterial, MATERIAL_MAP_DIFFUSE, tiles[1]);

			// NOTE transform would be easier if mesh origin was at center bottom probably?
			Matrix4x4 transform = MatrixScale(1.0f, 2.0f, 0.1f); // Scale box to wall shape
			WallTransformNorth = MatrixTranslate(0.0f, 1.0f, -0.5f) * transform; // Offset to edge of tile
			WallTransformWest = MatrixRotateY(MathF.PI / 2) * WallTransformNorth; // Rotate if west wall

		}

		private void LoadBrushes()
		{
			CreateBrush(new Brush(1, 1, 1, 1, 1, 1));
			CreateBrush(new Brush(1, 2, 3, 4, 5, 6));
		}

		private void LoadMeshes()
		{
			//string[] files = System.IO.Directory.GetFiles(AssetScenePrefix + "things", "*.thg");
		}

		public void LoadAssets()
		{
			// DONT CHANGE ORDER!
			LoadTiles();
			LoadUnits();
			LoadSprites();
			LoadShaders();
			GenerateUploadMeshes();
			LoadBrushes();
			LoadMeshes();
		}

		public bool WriteSceneToDisk(Scene scene, string filename = "huh.tac")
		{
			StringBuilder str = new StringBuilder();
			using (StreamWriter writer = new StreamWriter(Path.Combine(AssetSaveDirectory, filename))) {
				// Write Brushes
				writer.Write("[Brush]");
				// brushid = texture ids
				foreach (Brush brush in brushes) {
					str.AppendJoin(',', brush.faces);
					writer.WriteLine(str);
					str.Clear();
				}
				writer.WriteLine("[Units]");
				foreach (Unit unit in scene.units) {
					str.Append(unit.type);
					str.Append(',');
					str.Append(unit.name);
					str.Append(',');
					str.Append(unit.position.ToString());
					str.Append(',');
					str.Append(unit.direction.ToString());
					writer.Write(str);
					str.Clear();
				}

				writer.WriteLine("[Floor]");
				writer.Write(scene.floor.length);
				writer.Write(',');
				writer.WriteLine(scene.floor.width);
				writer.Write(',');
				foreach (Tile tile in scene.floor.map) {
					str.AppendJoin(',', tile.type, tile.North, tile.West, tile.thing, tile.walls);
					writer.Write(str);
					str.Clear();
				}
				writer.Write('\n');
			}

			return true;
		}

		public bool LoadSceneFromDisk(ref Scene scene, string filename)
		{
			using (StreamReader reader = new(Path.Combine(AssetSaveDirectory, filename))) {
				while (!reader.EndOfStream) {
					string line = reader.ReadLine();
					if (line[0] == '[') {

					}
				}
			}
			return true;
		}
	}
}
