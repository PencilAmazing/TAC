using Raylib_cs;

// This is why you don't make your own engine
// Wish I was doing XNA instead lmao
namespace TAC.Editor
{
	// Could be an interface but idgaf
	public class Resource
	{
		public readonly string assetname;
		protected Resource(string assetname)
		{
			this.assetname = assetname;
		}

		public override string ToString() => assetname;

	}

	public class Texture : Resource
	{
		public Texture2D texture;

		public Texture(string assetname, Texture2D texture)
			: base(assetname)
		{
			this.texture = texture;
		}
	}

	public class Model : Resource
	{
		// HACK model has built in materials, get rid of them
		// use meshes instead
		// but we need to stick to model because raylib stores skeletal
		// animation inside these structs
		public Raylib_cs.Model model;

		public Model(string assetname, Raylib_cs.Model model)
			: base(assetname)
		{
			this.model = model;
		}
	}
}
