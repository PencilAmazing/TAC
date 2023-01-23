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
	/// <summary>
	/// Manages and loads assets from disk.
	/// </summary>
	// Very spaghetti
	public partial class ResourceCache
	{
		// TODO remove repetition, make them configurable maybe too
		public readonly string AssetRootPrefix = "assets/";
		public readonly string AssetTilePrefix = "assets/tile/";
		public readonly string AssetBrushPrefix = "assets/brush/";
		public readonly string AssetUnitPrefix = "assets/unit/";
		public readonly string AssetShaderPrefix = "assets/shader/";
		public readonly string AssetScenePrefix = "assets/scene/";
		public readonly string AssetSaveDirectory = "assets/save/";

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

		// name, texture
		public Dictionary<string, Model> Models;
		public Dictionary<string, Texture> Textures;
		public Dictionary<string, Brush> Brushes;
		public Dictionary<string, Thing> Things;
		public Dictionary<string, UnitTemplate> UnitTemplates;

		// anim/file/name maybe?
		public Dictionary<string, ModelAnimation> ModelAnimations;

		// Discard transparent pixels and shift texture
		// Assumes that texcoords are either 0 or 1 only
		public Mesh BillboardMesh;
		public Material BillboardMaterial;
		public Shader BillboardShader;
		public int BillboardTexCoordShiftLoc;

		public Shader SkyboxShader;
		public Shader WallShader;
		public Shader TilemapShader;

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
			Models = new Dictionary<string, Model>();
			Textures = new Dictionary<string, Texture>();
			Brushes = new Dictionary<string, Brush>();
			Things = new Dictionary<string, Thing>();
			UnitTemplates = new Dictionary<string, UnitTemplate>();
			ModelAnimations = new Dictionary<string, ModelAnimation>();
		}

		~ResourceCache()
		{
			foreach (Texture tex in Textures.Values)
				UnloadTexture(tex.texture);

			UnloadShader(BillboardShader);
			UnloadShader(SkyboxShader);

			// Should also unload MemAllocated vertices
			UnloadMesh(ref cube);
			UnloadMaterial(wallMaterial);
			UnloadMaterial(SkyboxMaterial);
			UnloadTexture(SkyboxCubemap);
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
			Raylib.GenTextureMipmaps(ref tex);
			SetTextureFilter(tex, TextureFilter.TEXTURE_FILTER_TRILINEAR);
			SetTextureWrap(tex, TextureWrap.TEXTURE_WRAP_CLAMP);
			

			return tex.id == 0 ? null : LoadTexture(tex, TextureKey);
		}

		/// <summary>
		/// Manual insert texture into cache
		/// </summary>
		public Texture LoadTexture(Texture2D tex, string TextureKey)
		{
			Texture loadedTexture = new Texture(TextureKey, tex);
			Textures[TextureKey] = loadedTexture;
			return loadedTexture;
		}

		private void LoadDefaultAssets()
		{
			Texture transparentTexture;
			Image i = GenImageColor(128, 128, Color.BLANK);
			Texture2D tex = LoadTextureFromImage(i);
			UnloadImage(i);
			transparentTexture = LoadTexture(tex, "tile/null");
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

			TilemapShader = LoadShader(null, AssetShaderPrefix + "tilemap.frag");
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

			// Skybox
			{
				SkyboxMaterial = LoadMaterialDefault();
				SkyboxMaterial.shader = SkyboxShader;
				Image SkyboxImage = LoadImage(AssetScenePrefix + "skybox/skybox.png");
				SkyboxCubemap = LoadTextureCubemap(SkyboxImage, CubemapLayout.CUBEMAP_LAYOUT_AUTO_DETECT);
				UnloadImage(SkyboxImage);
				SetMaterialTexture(ref SkyboxMaterial, MATERIAL_MAP_CUBEMAP, SkyboxCubemap);
			}
			// Crosshair particle mesh
			{
				// Front faces are CCW although disable culling when drawing either way
				// TODO add a YZ plane too
				// TODO make scale of particles a gpu uniform
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
			}
			// Billboard model
			{
				BillboardMaterial = LoadMaterialDefault();
				BillboardMaterial.shader = BillboardShader;

				float[] vertices =
				{
					// XY plane
					-0.5f, 0.0f, 0,
					-0.5f, 1.0f, 0,
					0.5f, 0.0f, 0,
					0.5f, 1.0f, 0
				};
				float[] normals =
				{
					0, 0, 1,
					0, 0, 1,
					0, 0, 1,
					0, 0, 1
				};
				float[] texcoords =
				{
					0, 0,
					0, 1,
					1, 0,
					1, 1,
				};
				ushort[] indices = { 0, 2, 1, 1, 2, 3 };

				unsafe {
					fixed (Mesh* mesh = &BillboardMesh) {
						// Plane. 4 vertices two triangles
						AllocateMeshData(mesh, 4, 2);
					}
					vertices.CopyTo(new Span<float>(BillboardMesh.vertices, vertices.Length));
					normals.CopyTo(new Span<float>(BillboardMesh.normals, normals.Length));
					texcoords.CopyTo(new Span<float>(BillboardMesh.texcoords, texcoords.Length));
					indices.CopyTo(new Span<ushort>(BillboardMesh.indices, indices.Length));
				}

				UploadMesh(ref BillboardMesh, false);

			}
			// Wall
			{
				wallMaterial = LoadMaterialDefault();
				wallMaterial.shader = WallShader;

				// NOTE transform would be easier if mesh origin was at center bottom probably?
				Matrix4x4 transform = MatrixScale(1.0f, 2.0f, 0.1f); // Scale box to wall shape
				WallTransformNorth = MatrixTranslate(0.0f, 1.0f, -0.5f) * transform; // Offset to edge of tile
				WallTransformWest = MatrixRotateY(MathF.PI / 2) * WallTransformNorth; // Rotate if west wall
			}
		}

		public void LoadAssets()
		{
			// DONT CHANGE ORDER!
			LoadDefaultAssets();
			LoadSprites();
			LoadShaders();
			GenerateUploadMeshes();
		}

		/// <summary>
		/// Dont forget the extension i guess
		/// </summary>
		public bool AssetExists(string assetname)
		{
			if (System.IO.File.Exists(AssetRootPrefix + assetname)) return true;
			TraceLog(TraceLogLevel.LOG_ERROR, "Asset " + assetname + " not found.");
			return false;
		}

		/// <summary>
		/// Make sure extension has dot. <br></br>
		/// Simple concatenation as AssetRootPrefix + assetname + suffix
		/// </summary>
		public string GetFullAssetPath(string assetname, string suffix) => System.IO.Path.GetFullPath(AssetRootPrefix + assetname + suffix);

		public Brush GetBrush(string assetname)
		{
			if (Brushes.ContainsKey(assetname)) {
				return Brushes[assetname];
			} else return LoadBrush(assetname);
		}

		public Brush LoadBrush(string assetname)
		{
			if (!System.IO.File.Exists(AssetRootPrefix + assetname + ".json")) return null;
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

		public Thing GetThing(string assetname)
		{

			if (Things.ContainsKey(assetname)) return Things[assetname];

			if (!System.IO.File.Exists(AssetRootPrefix + assetname + ".json")) return null;
			string filelocation = System.IO.Path.GetFullPath(AssetRootPrefix + assetname + ".json");
			string file = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonNode node = JsonNode.Parse(file);

			string modelName = node["model"].ToString();
			Model thingModel = GetModel(modelName);

			JsonNode edit = node["edit"];
			if (edit != null) {
				// Scale rotate transform
				float scalex = (float)edit["scale"][0];
				float scaley = (float)edit["scale"][1];
				float scalez = (float)edit["scale"][2];
				thingModel.model.transform *= Raymath.MatrixScale(scalex, scaley, scalez);
				float transx = (float)edit["transform"][0];
				float transy = (float)edit["transform"][1];
				float transz = (float)edit["transform"][2];
				thingModel.model.transform *= Raymath.MatrixTranslate(transx, transy, transz);
			}

			bool blockSight = (bool)node["blockSight"];
			bool blockAim = (bool)node["blockAim"];
			bool blockPath = (bool)node["blockPath"];
			Thing thing = new Thing(assetname, thingModel, blockSight, blockAim, blockPath);

			return thing;
		}

		public Model GetModel(string assetname)
		{
			if (Models.ContainsKey(assetname)) return Models[assetname];

			string extension;
			if (AssetExists(assetname + ".iqm")) {
				extension = ".iqm";
			} else if (AssetExists(assetname + ".obj")) {
				extension = ".obj";
			} else return null;

			Raylib_cs.Model obj = Raylib.LoadModel(GetFullAssetPath(assetname, extension));
			// Error check
			if (obj.meshCount <= 0) {
				Raylib.UnloadModel(obj);
				return null;
			}

			// The fuck is going on here
			// Hey guess fucking what LoadMaterials doesn't even do shit
			// I hate this so much man
			/*if (extension == ".iqm") {
				unsafe {
					int count = 0;
					using var path = GetFullAssetPath(assetname, ".mtl").ToUTF8Buffer();
					Material* mats;
					//Material* mat = LoadMaterials((sbyte*)p, &count);
					mats = LoadMaterials(path.AsPointer(), &count);
					if (count > 0) {
						obj.materials = mats;
					}
				}
			}*/
			// Load and register model
			Model model = new Model(assetname, obj);
			Models.Add(assetname, model);
			return model;
		}

		/// <summary>
		/// IQM file could contain multiple animations
		/// </summary>
		public ModelAnimation GetModelAnimation(string assetname)
		{
			// anim/biped/runN
			if (ModelAnimations.ContainsKey(assetname)) return ModelAnimations[assetname];
			// Remove animation name to get file assetname
			// anim/biped
			string animFileName = assetname.Remove(assetname.LastIndexOf('/'));
			if (!AssetExists(animFileName + ".json")) return null;

			string filelocation = System.IO.Path.GetFullPath(AssetRootPrefix + animFileName + ".json");
			string file = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonNode node = JsonNode.Parse(file);

			// runN
			JsonArray animationorder = node["animation_order"].AsArray();
			uint animCount = 0;
			ReadOnlySpan<Raylib_cs.ModelAnimation> anims = Raylib.LoadModelAnimations(GetFullAssetPath(animFileName, ".iqm"), ref animCount);
			// Early out since some animations are loaded without assetnames
			if (animationorder == null || animationorder.Count < animCount) {
				UnloadModelAnimations(anims.ToArray(), animCount);
				return null;
			}

			// Construct assetname and load into registry
			//for (int i = 0; i < animationorder.Count; i++) {
			for (int i = 0; i < animCount; i++) {
				// new assetname
				string asset = animFileName + '/' + (string)animationorder[i];
				ModelAnimations.Add(asset, new ModelAnimation(assetname, anims[i]));
			}
			
			if (ModelAnimations.ContainsKey(assetname)) // anim should be loaded by now
				return ModelAnimations[assetname];
			else // nothing found
				return null;
		}
	}
}
