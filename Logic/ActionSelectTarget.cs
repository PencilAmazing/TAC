using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Logic
{
	public class ActionSelectTarget : Action
	{
		private Item item;
		private Unit start;
		private Position target;
		public List<Position> line;

		public ActionSelectTarget(Scene scene, Unit start, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.start = start;
			this.target = target;
			this.line = scene.GetSupercoverLine(start.position, target);
		}

		private void ThinkStraight(float dt)
		{
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);
			if (item.projectileType == ProjectileType.Straight) {
				ThinkStraight(deltaTime);
			} else if (item.projectileType == ProjectileType.Gravity) {
				Done();
			} else {
				Done();
			}

		}
	}
}
