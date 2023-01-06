﻿using Raylib_cs;

namespace TAC.Editor
{
	public class Texture
	{
		public readonly string assetname;
		public readonly Texture2D tex;

		public Texture(string textureName, Texture2D tex)
		{
			assetname = textureName;
			this.tex = tex;
		}

		public override string ToString() => assetname;
	}

	// This is why you don't make your own engine
	//public class Model {
	//	public readonly string assetname;
	//	public readonly Material material;
	//}
}
