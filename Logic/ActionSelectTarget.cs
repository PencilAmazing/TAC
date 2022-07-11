using System.Numerics;
using TAC.World;

namespace TAC.Logic
{
	public class ActionSelectTarget : Action
	{
		private Item item;
		private Unit start;
		private Position target;
		public Position[] line;
		private ParticleEffect impactEffect;

		public ActionSelectTarget(Scene scene, Unit start, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.start = start;
			this.start.phase = 0;
			this.target = target;
			line = scene.GetSupercoverLine(start.position, target);
			impactEffect = new ParticleEffect(item.impactEffect, 12, target.ToVector3(), Vector2.One);

		}

		private void ThinkStraight(float dt)
		{
			if (start.phase == 0) {
				//scene.AddParticleEffect(actionEffect);
			} else if (start.phase / 8 < 4) {
				//actionEffect.position = Vector3.Lerp(start.position.ToVector3(), target.ToVector3(), start.phase * 4 / 8);
			} else if (start.phase / 8 == 4) {
				scene.AddParticleEffect(impactEffect);
			} else if (start.phase / 8 < 8) {
				impactEffect.phase += 1;
			} else if (start.phase / 8 <= 12) {
				Done();
			}
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
			start.phase += 1;
		}

		public override void Done()
		{
			base.Done();
			impactEffect.Done();
		}
	}
}
