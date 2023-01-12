using TAC.Editor;

namespace TAC.World
{
	// Thing is more funny than object
	/// <summary>
	/// Resource cache representation of an object in scene
	/// </summary>
	public class Thing
	{
		public string assetname { get; }
		/// <summary> Resource mesh reference. </summary>
		// FIXME replace with our own model class
		public Model model;
		/// <summary> Does it block sight?</summary>
		public bool blockSight;
		/// <summary>  Does it block aim? </summary>
		public bool blockAim;
		/// <summary> Does it block pathfinding? </summary>
		public bool blockPath;

		public Thing(string assetname, Model model, bool blockSight = false, bool blockAim = false, bool blockPath = false)
		{
			this.assetname = assetname;
			this.model = model;
			this.blockSight = blockSight;
			this.blockAim = blockAim;
			this.blockPath = blockPath;
		}

		public void Think(float deltaTime)
		{
			// animations? in my video game? more likely than you think!
		}
	}
}
