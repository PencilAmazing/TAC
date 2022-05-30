using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Logic
{
	abstract class Action
	{
		protected Scene scene;
		public bool isDone;

		public Action(Scene scene)
		{
			this.scene = scene;
			this.isDone = false;
		}
		virtual public void Think(float deltaTime)
		{
			return;
		}
	}
}
