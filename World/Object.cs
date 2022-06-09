using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAC.World
{
	public class Thing
	{
		// Resource mesh reference
		public int modelid { get; }
		/// <summary> Does it occlude vision? </summary>
		public bool occluder { get; }
		/// <summary>  Does it block line of sight? </summary>
		public bool blocker { get; }
		/// <summary> Does it block pathfinding? </summary>
		public bool obstacle { get; }

		public Thing(int modelid, bool occluder = false, bool blocker = false, bool obstacle = false)
		{
			this.modelid = modelid;
			this.occluder = occluder;
			this.blocker = blocker;
			this.obstacle = obstacle;
		}

		public void Think(float deltaTime)
		{
			// animations? in my video game? more likely than you think!
		}
	}
}
