using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;
using TAC.UISystem;
using static ImGuiNET.ImGui;
// Ew static classes. But who owns UI and how will it reach here?

namespace TAC.Logic
{
	public class ActionOutOfTime : Action
	{
		public ActionOutOfTime(Scene scene) : base(scene)
		{
		}

		// Display message up top notifying user that unit is stranded lmao
		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);
			phase += 1;
			Begin("OutOfTimeWindow");
				Text("Unit out of time!");
			End();

			if (phase >= 100) Done();
		}

	}
}
