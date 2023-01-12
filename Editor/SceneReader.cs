using Raylib_cs;

namespace TAC.Editor
{
	public partial class ResourceCache
	{
		public Model GetModel(string assetname)
		{
			if (Models.ContainsKey(assetname)) return Models[assetname];

			// Check file exists
			if (!AssetExists(assetname + ".obj")) return null;

			Raylib_cs.Model obj = Raylib.LoadModel(GetFullAssetPath(assetname, ".obj"));
			// Error check
			if (obj.meshCount <= 0) {
				Raylib.UnloadModel(obj);
				return null;
			}

			// Load and register model
			Model model = new Model(assetname, obj);
			Models.Add(assetname, model);
			return model;
		}
	}
}
