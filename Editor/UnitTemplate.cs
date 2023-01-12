using Raylib_cs;

namespace TAC.Editor
{
	/// <summary>
	/// Describes default values for a Unit
	/// </summary>
	public class UnitTemplate : Resource
	{
		public enum TemplateType
		{
			Skeletal,
			Billboard
		};

		/*
		 * If it's a billboard then we're creating a different model
		 * anyways that references the same mesh. We then set different materials
		 * based on that model
		 * for now
		 */

		public TemplateType Type { get; private set; }
		public int Health;
		public int TimeUnits;
		public Model Model;
		public Texture BillboardTexture;

		/// <summary>
		/// Create a skeletal Unit Template
		/// </summary>
		public UnitTemplate(string assetname, int health, int timeUnits, Model model)
			: base(assetname)
		{
			Health = health;
			TimeUnits = timeUnits;
			Model = model;
			BillboardTexture = null;

			Type = TemplateType.Skeletal;
		}

		public UnitTemplate(string assetname, int health, int timeUnits, Texture texture)
			: base(assetname)
		{
			Health = health;
			TimeUnits = timeUnits;
			Model = null;
			BillboardTexture = texture;

			Type = TemplateType.Billboard;
		}
	}
}
