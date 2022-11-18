using System.Numerics;
using TAC.World;

namespace TAC.Logic
{
	class ActionTargetImpact : Action
	{
		private ParticleEffect impactEffect;
		private int phase; // TODO move this to base class maybe

		public ActionTargetImpact(Scene scene, Item item, Vector3 impact) : base(scene)
		{
			this.phase = 0;
			impactEffect = new ParticleEffect(item.impactEffect, 12, impact, Vector2.One);
			scene.AddParticleEffect(impactEffect);
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			if (phase / 8 <= 4) {
				impactEffect.phase += 1;
			} else {
				Done();
			}

			phase += 1;
		}

		public override void Done()
		{
			base.Done();
			impactEffect.Done();
		}
	}
}
