using System.Numerics;
using TAC.World;

namespace TAC.Logic
{
	public struct TargetImpactData
	{
		public bool hit;
		// World space hit point
		public Vector3 Point;
		// Tile containing collision thing, wall
		public Position Tile;
		/// <summary>
		/// Type of collison. 0 is thing, 1 is north wall, 2 is west wall, 3 is unit
		/// </summary>
		public Wall HitType;

		public TargetImpactData(bool hit, Vector3 point, Position tile, Wall hitType)
		{
			this.hit = hit;
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

		public ActionTargetImpact(Scene scene, Item item, TargetImpactData impact) : base(scene)
		{
			this.impact = impact;
			impactEffect = new ParticleEffect(item.impactEffect, 12, impact.Point, Vector3.One * 2, Vector3.Zero);
			scene.AddParticleEffect(impactEffect);
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);
			if (phase == 0) {
				// NOTE maybe impact has radius?
				if (scene.IsTileWithinBounds(new Position(impact.Point))) {

					if ((int)impact.HitType == 0) {
						// Thing hit
					} else if (impact.HitType == Wall.North || impact.HitType == Wall.West) {
						scene.ClearBrush(impact.Tile, impact.HitType);
					} else if ((int)impact.HitType == 3) {
						scene.AffectUnit(scene.GetUnit(impact.Tile), -10);
					}
				}
			} else if (phase / 8 <= 4) {
				impactEffect.phase += 1;
			} else Done();

			phase += 1;
		}

		public override void Done()
		{
			base.Done();
			impactEffect.Done();
		}
	}
}
