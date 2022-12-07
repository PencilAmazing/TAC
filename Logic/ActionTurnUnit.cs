using ImGuiNET;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Logic
{
	/// <summary>
	/// Turn around a unit. Searches for shortest distance to minimize turns
	/// </summary>
	public class ActionTurnUnit : Action
	{
		private readonly Unit unit;
		private readonly UnitDirection targetDirection;

		/// <summary>
		/// Turns a unit around, costs 1 time unit.
		/// Can return ActionOutOfTime instead of nextAction
		/// </summary>
		public ActionTurnUnit(Scene scene, Unit unit, UnitDirection targetDirection) : base(scene)
		{
			this.scene = scene;
			this.unit = unit;
			this.targetDirection = targetDirection;
			this.TimeCost = 1;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			int delta = Pathfinding.GetDirectionDelta(unit.direction, targetDirection);

			phase += 1;
			if (phase > 8) {
				if (delta == 0) { // Still have more turns to do
					Done();
					return;
				}
				if (unit.TimeUnits < TimeCost) {
					nextAction = new ActionOutOfTime(scene); // Can't afford it
					Done();
					return;
				}

				phase = 0;
				UnitDirection nextDirection = (UnitDirection)(((int)unit.direction + Math.Sign(delta) + 8) % 8);
				unit.direction = nextDirection;
				unit.TimeUnits -= TimeCost;
			}
		}
	}
}
