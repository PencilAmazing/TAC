using ImGuiNET;
using System;
using System.Reflection.Metadata;
using TAC.World;

namespace TAC.Logic
{
	/// <summary>
	/// Turn around a unit. Searches for shortest distance to minimize turns
	/// </summary>
	public class ActionTurnUnit : Action
	{
		private Unit unit;
		private UnitDirection targetDirection;

		private int phase; // TODO move to base class

		public ActionTurnUnit(Scene scene, Unit unit, UnitDirection targetDirection) : base(scene)
		{
			this.scene = scene;
			this.unit = unit;
			this.targetDirection = targetDirection;

			this.phase = 0;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			// https://math.stackexchange.com/a/2898118
			// Distance of target enum from current ennum
			int delta = ((int)targetDirection - (int)unit.direction + 12) % 8 - 4;

			phase += 1;
			if (phase > 8) {
				if (delta == 0) Done();
				phase = 0;
				UnitDirection nextDirection = (UnitDirection)(((int)unit.direction + Math.Sign(delta)) % 8);
				unit.direction = nextDirection;
				delta -= 1;
			}
		}
	}
}
