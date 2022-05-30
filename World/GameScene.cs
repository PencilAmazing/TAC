using System.Collections.Generic;
using TAC.Render;
using TAC.Logic;
using Raylib_cs;

namespace TAC.World
{
	class GameScene : Scene
	{
		public List<Unit> units { get; }

		private Action currentAction;

		public GameScene(Position size, Renderer renderer, ResourceCache cache)
			: base(size, renderer, cache)
		{
			units = new List<Unit>();

			currentAction = null;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			foreach (Unit unit in units)
				unit.Think(deltaTime);

			if (currentAction != null) {
				currentAction.Think(deltaTime);
				if (currentAction.isDone) currentAction = null;
			}
		}

		public override void Draw(Camera3D camera)
		{
			base.Draw(camera);

			renderer.DrawUnits(camera, units, cache);
			renderer.DrawUnitDebug(camera, units, cache);
		}

		public override void DrawDebug3D(Camera3D camera)
		{
			base.DrawDebug3D(camera);
			ActionMoveUnit action = currentAction as ActionMoveUnit;
			if (action != null)
				renderer.DrawDebugPath(action.path.path.ToArray());
		}

		public void AddUnit(Unit unit)
		{
			if (unit == null)
				return;
			units.Add(unit);
			Tile tile = floor.GetTile(unit.position.x, unit.position.z);
			tile.unit = unit;
			floor.SetTile(unit.position, tile);
		}

		public void MoveUnit(Unit unit, Position goal)
		{
			if (unit == null) return;
			Pathfinding path = new Pathfinding(this);
			if (path.FindPathForUnit(unit, goal)) {
				currentAction = new ActionMoveUnit(this, path);
			}
		}

		public Unit GetUnit(Position pos)
		{
			return floor.GetTile(pos.x, pos.z).unit;
		}
	}
}
