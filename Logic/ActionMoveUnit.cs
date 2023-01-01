using System;
using TAC.World;

namespace TAC.Logic
{
	class ActionMoveUnit : Action
	{
		public Pathfinding path;

		public ActionMoveUnit(Scene scene, Pathfinding path) : base(scene)
		{
			this.path = path;
			this.TimeCost = 4;
			if (path == null) Done();
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			path.unit.phase += 1;
			if (path.unit.phase > 8) {
				path.unit.phase = 0;

				// Turning takes a whole phase loop
				// Handle turning by yourself here since switching to a whole
				// another action simply isn't worth it,
				UnitDirection targetDir = Pathfinding.GetDirection(path.unit.position, path.path[0]);
				if (path.unit.direction != targetDir) {
					int delta = Pathfinding.GetDirectionDelta(path.unit.direction, targetDir);
					if (path.unit.TimeUnits >= 1) {
						// Turn around
						UnitDirection nextDirection = (UnitDirection)(((int)path.unit.direction + Math.Sign(delta) + 8) % 8);
						path.unit.direction = nextDirection;
						path.unit.TimeUnits -= 1;
					} else {
						// Out of time, abort action
						SetNextAction(new ActionOutOfTime(scene));
						Done();
					}
					return;
				}

				// Still has more steps to go but not enough time
				if (path.path.Count > 0 && path.unit.TimeUnits < 4) {
					SetNextAction(new ActionOutOfTime(scene));
					Done();
					return;
				}

				// Advance unit
				scene.MoveUnitToTile(path.unit, path.path[0]);
				path.path.RemoveAt(0);

				path.unit.TimeUnits -= 4;
				if (path.path.Count == 0) Done();
			}
		}

	}
}
