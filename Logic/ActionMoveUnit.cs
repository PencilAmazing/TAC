using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Logic
{
	class ActionMoveUnit : Action
	{
		public Pathfinding path;

		public ActionMoveUnit(Scene scene, Pathfinding path) : base(scene)
		{
			this.path = path;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			path.unit.phase += 1;
			if (path.unit.phase > 8) {
				path.unit.phase = 0;
				// Advance unit
				scene.floor[path.unit.position.x, path.unit.position.z].unit = null;

				UnitDirection dir = Pathfinding.GetDirection(path.unit.position, path.path[0]);
				path.unit.direction = dir;
				
				path.unit.position = path.path[0];
				path.path.RemoveAt(0);
				scene.floor[path.unit.position.x, path.unit.position.z].unit = path.unit;
				if (path.path.Count == 0) isDone = true;
			}
		}

	}
}
