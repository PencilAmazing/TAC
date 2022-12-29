using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Logic
{
	public class UnitAIModule
	{
		private string NameID;
		private Unit unit;
		private Scene scene;

		private bool actionDecided;

		public UnitAIModule(Scene scene, Unit unit)
		{
			this.scene = scene;
			this.unit = unit;
			actionDecided = false;
		}

		public Action Think()
		{
			// Dummy unit, always done
			unit.isDone = true;
			actionDecided = true;
			Pathfinding path = new Pathfinding(scene);
			Random rand = new();
			Position goal;
			goal.x = rand.Next(0, 13);
			goal.y = 0;
			goal.z = rand.Next(0, 13);
			path.FindPathForUnit(unit, goal);
			return new ActionMoveUnit(scene, path);
		}

		public bool StillThinking()
		{
			// return true if not done yet and still has time to move around
			return !unit.isDone && unit.TimeUnits > 4;
		}

		public void Reset()
		{
			actionDecided = false;
			return;
		}
	}
}
