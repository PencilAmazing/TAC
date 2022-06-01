using Raylib_cs;
using System.Collections.Generic;
using static Raylib_cs.MaterialMapIndex;
using static Raylib_cs.Raylib;
using static Raylib_cs.ShaderUniformDataType;

namespace TAC.Render
{
	// Maps textures loaded in memory to tile IDs
	class ResourceCache
	{
		private string AssetTilePrefix = "assets/tile/";
		private string AssetUnitPrefix = "assets/unit/";
		private string AssetShaderPrefix = "assets/shader/";
		private string AssetScenePrefix = "assets/scene/";

		// promise me you won't touch anything
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
			TEX_MISC
		}

		// Might be inefficient, whatever
		public List<Texture2D> tiles { get; }
		public List<Texture2D> units { get; }
		public List<Texture2D> items { get; }

		public Shader BillboardShader;
		public Shader SkyboxShader;
		public Shader WallShader;

		// General purpose cube mesh
		public Mesh cube;
		public Material wallMaterial;

		public Material SkyboxMaterial;
		public Texture2D SkyboxCubemap;

		public ResourceCache()
		{
			tiles = new List<Texture2D>();
			units = new List<Texture2D>();
			items = new List<Texture2D>();
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
					break;
				default:
					break;
			}
			return -1;
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

		private void LoadShaders()
		{
			BillboardShader = LoadShader(null, AssetShaderPrefix + "billboard.fs");
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

		private void UploadMeshes()
		{
			// GenMesh uploads data to GPU
			cube = GenMeshCube(1, 1, 1);
			SkyboxMaterial = LoadMaterialDefault();
			SkyboxMaterial.shader = SkyboxShader;

			Image SkyboxImage = LoadImage(AssetScenePrefix + "skybox/skybox.png");
			SkyboxCubemap = LoadTextureCubemap(SkyboxImage, CubemapLayout.CUBEMAP_LAYOUT_AUTO_DETECT);
			UnloadImage(SkyboxImage);
			SetMaterialTexture(ref SkyboxMaterial, MATERIAL_MAP_CUBEMAP, SkyboxCubemap);

			wallMaterial = LoadMaterialDefault();
			wallMaterial.shader = WallShader;
			//SetMaterialTexture(ref wallMaterial, MATERIAL_MAP_DIFFUSE, tiles[1]);
		}

		public void LoadAssets()
		{
			// DONT CHANGE ORDER!
			LoadTiles();
			LoadUnits();
			LoadShaders();
			UploadMeshes();
		}
	}
}
