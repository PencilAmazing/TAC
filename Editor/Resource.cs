using Raylib_cs;

namespace TAC.Editor
{
	public class Texture
	{
		public readonly string TextureName;
		public readonly Texture2D tex;

		public Texture(string textureName, Texture2D tex)
		{
			TextureName = textureName;
			this.tex = tex;
		}

		public override string ToString() => TextureName;
	}
}
