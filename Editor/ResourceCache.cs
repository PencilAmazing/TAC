using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using TAC.World;
using static Raylib_cs.MaterialMapIndex;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using static Raylib_cs.ShaderUniformDataType;

namespace TAC.Editor
{
	// Maps textures loaded in memory to tile IDs
	public class ResourceCache
	{
		private string AssetRootPrefix = "assets/";
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

		public Dictionary<string, UnitTemplate> UnitTemplates;

		// name, texture
		public Dictionary<string, Texture> Textures;
		public Dictionary<string, Brush> Brushes;

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
			Textures = new Dictionary<string, Texture>();
			Brushes = new Dictionary<string, Brush>();
			UnitTemplates = new Dictionary<string, UnitTemplate>();
		}

		~ResourceCache()
		{
			foreach (Texture tex in Textures.Values)
				UnloadTexture(tex.tex);

			UnloadShader(BillboardShader);
			UnloadShader(SkyboxShader);

			// Should also unload MemAllocated vertices
			UnloadMesh(ref cube);
			UnloadMaterial(wallMaterial);
			UnloadMaterial(SkyboxMaterial);
			UnloadTexture(SkyboxCubemap);

			foreach (Model model in things)
				UnloadModel(model);

		}

		/// <summary>
		/// Takes texture key, returns reference to loaded texture or null
		/// </summary>
		public Texture GetTexture(string TexturePath)
		{
			if (Textures.ContainsKey(TexturePath)) {
				return Textures[TexturePath];
			} else return LoadTexture(TexturePath);
		}

		public Texture LoadTexture(string TextureKey)
		{
			if (!System.IO.File.Exists(AssetRootPrefix + TextureKey + ".png")) return null;
			Texture2D tex = Raylib.LoadTexture(AssetRootPrefix + TextureKey + ".png");
			SetTextureFilter(tex, TextureFilter.TEXTURE_FILTER_POINT);
			SetTextureWrap(tex, TextureWrap.TEXTURE_WRAP_CLAMP);

			string TextureName = TextureKey;
			if (tex.id == 0)
				return null;
			else {
				Texture loadedTexture = new Texture(TextureName, tex);
				Textures[TextureName] = loadedTexture;
				return loadedTexture;
			}
		}

		private void LoadTiles()
		{
			//LoadTexture(AssetTilePrefix + "OBKMTB90.png");
			//LoadTexture(AssetTilePrefix + "OBASEM37.png");
			//LoadTexture(AssetTilePrefix + "OBOOKA03.png");
			//LoadTexture(AssetTilePrefix + "OBRCKL02.png");
			//LoadTexture(AssetTilePrefix + "OBRCKQ12.png");
			//LoadTexture(AssetTilePrefix + "OBRCKQ44.png");
			//LoadTexture(AssetTilePrefix + "OCHRMA14.png");
		}

		private void LoadUnits()
		{
			//LoadTexture(AssetUnitPrefix + "mech.png");
		}

		private void LoadSprites()
		{
			LoadTexture("scene/sprite/explosion_11");
			LoadTexture("scene/sprite/ProjectileArranged");
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
			//LoadBrush("brush/copper");
			//CreateBrush("rainbow", new Brush(1, 2, 3, 4, 5, 6));
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
		}

		public UnitTemplate GetUnitTemplate(string assetname)
		{
			string filelocation = System.IO.Path.GetFullPath(AssetRootPrefix + assetname + ".json");
			string unitFile = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonNode templateNode = JsonNode.Parse(unitFile);

			Texture tex = GetTexture(templateNode["texture"].GetValue<string>());
			UnitTemplate template = new UnitTemplate((int)templateNode["health"], (int)templateNode["time"], tex);
			UnitTemplates.Add(assetname, template);

			return template;
		}

		public Brush GetBrush(string assetname)
		{
			if (Textures.ContainsKey(assetname)) {
				return Brushes[assetname];
			} else return LoadBrush(assetname);
		}

		public Brush LoadBrush(string assetname)
		{
			string filelocation = System.IO.Path.GetFullPath(AssetRootPrefix + assetname + ".json");
			string brushFile = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonNode brushNode = JsonNode.Parse(brushFile);

			JsonArray faces = brushNode["faces"].AsArray();
			Texture[] textures = new Texture[6];

			for (int i = 0; i < 6; i++)
				textures[i] = GetTexture((string)faces[i]);

			Brush brush = new Brush(assetname, textures);
			Brushes.Add(assetname, brush);

			return brush;
		}
	}
}
