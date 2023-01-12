using System.IO;
using System.Text.Json;
using TAC.World;

namespace TAC.Editor
{
	public partial class ResourceCache
	{
		/// <summary>
		/// Exports scene as json. True if succeeded
		/// </summary>
		public bool WriteSceneToDisk(Scene scene, string assetname)
		{
			//https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-dom-utf8jsonreader-utf8jsonwriter?pivots=dotnet-6-0#use-jsondocument-to-write-json
			var writerOptions = new JsonWriterOptions
			{
				Indented = true // Maybe not?
			};
			string jsonData = scene.GetJsonNode().ToJsonString(new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(AssetSaveDirectory + assetname + ".json", jsonData);
			Raylib_cs.Raylib.TraceLog(Raylib_cs.TraceLogLevel.LOG_INFO, "Wrote to disk scene: " + assetname);
			return true;
		}
	}
}
