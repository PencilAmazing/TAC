using Raylib_cs;
using System.Text.Json.Nodes;
using System.Text;
using TAC.World;
using System.Numerics;
using System;

namespace TAC.Editor
{
	public partial class ResourceCache
	{
		private void ParseUnitTemplateMaterials(JsonArray texarray, ref Raylib_cs.Model model)
		{
			for (int i = 0; i < texarray.Count; i++) {
				Texture2D tex = Raylib.LoadTexture(AssetUnitPrefix + (string)texarray[i]);
				Raylib.SetMaterialTexture(ref model, i, MaterialMapIndex.MATERIAL_MAP_DIFFUSE, ref tex);
				Raylib.SetModelMeshMaterial(ref model, i*2, i);
			}
		}

		public UnitTemplate GetUnitTemplate(string assetname)
		{
			if (UnitTemplates.ContainsKey(assetname)) return UnitTemplates[assetname];

			string filelocation = System.IO.Path.GetFullPath(AssetRootPrefix + assetname + ".json");
			string unitFile = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonNode templateNode = JsonNode.Parse(unitFile);

			UnitTemplate template;
			if (templateNode["texture"] != null) {
				Texture tex = GetTexture(templateNode["texture"].GetValue<string>());
				template = new UnitTemplate(assetname, (int)templateNode["health"], (int)templateNode["time"], tex);
			} else if (templateNode["model"] != null) {
				Model templateModel = GetModel((string)templateNode["model"]);
				JsonArray texarray = templateNode["textures"].AsArray();
				ParseUnitTemplateMaterials(texarray, ref templateModel.model);
				// scale rotate translate
				templateModel.model.transform = Raymath.MatrixScale(0.02f, 0.02f, 0.02f) *
												Raymath.MatrixRotate(Vector3.UnitX, -MathF.PI/2.0f) *
												templateModel.model.transform;
				template = new UnitTemplate(assetname, (int)templateNode["health"], (int)templateNode["time"], templateModel);
			} else {
				Raylib.TraceLog(TraceLogLevel.LOG_INFO, "Unit template " + assetname + " does not specify template type.");
				return null; // Early out since definition is already broken
			}

			JsonNode animarray = templateNode["animations"];
			if (animarray != null) {
				template.Animations = new ModelAnimation[animarray.AsArray().Count];
				for (int i = 0; i < animarray.AsArray().Count; i++) {
					template.Animations[i] = GetModelAnimation((string)animarray[i]);
				}
			}

			UnitTemplates.Add(assetname, template);
			return template;
		}

		public Scene GetScene(string assetname)
		{
			string filelocation = System.IO.Path.GetFullPath(AssetSaveDirectory + assetname + ".json");
			string unitFile = System.IO.File.ReadAllText(filelocation, Encoding.UTF8);
			JsonObject sceneNode = JsonNode.Parse(unitFile).AsObject();

			Scene outScene = new Scene(null, this, false);
			outScene.FillFromJson(sceneNode);

			return outScene;
		}
	}
}
