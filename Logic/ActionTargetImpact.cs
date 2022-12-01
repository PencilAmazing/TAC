using System.Numerics;
using TAC.World;

namespace TAC.Logic
{
	public struct TargetImpactData
	{
		// World space hit point
		public Vector3 Point;
		// Tile containing collision thing, wall
		public Position Tile;
		// Type of collison. 0 is thing, 1 is north wall, 2 is west wall
		public Wall HitType;

		public TargetImpactData(Vector3 point, Position tile, Wall hitType)
		{
			Point = point;
			Tile = tile;
			HitType = hitType;
		}
		// NOTE Can a tile contain multiple things?
		// if so we might want a separate Thing field in here
		//public Thing HitThing
		//public Wall HitWall
	}

	class ActionTargetImpact : Action
	{
		private TargetImpactData impact;

		private ParticleEffect impactEffect;
		private int phase; // TODO move this to base class maybe

		public ActionTargetImpact(Scene scene, Item item, TargetImpactData impact) : base(scene)
		{
			this.phase = 0;
			this.impact = impact;
			impactEffect = new ParticleEffect(item.impactEffect, 12, impact.Point, Vector2.One);
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
