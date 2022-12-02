using System.Numerics;
using TAC.World;
using Raylib_cs;
using TAC.Editor;
using static Raylib_cs.Raymath;
using System;

namespace TAC.Logic
{

	public class ActionSelectTarget : Action
	{
		private Item item;
		public Unit start;
		public Position target;
		public Position[] line;

		public RayCollision collision;
		public TargetImpactData impactData;

		private ParticleEffect actionEffect;

		public ActionSelectTarget(Scene scene, Unit start, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.start = start;
			this.start.phase = 0;
			this.target = target;
			Vector3 chestHeight = start.position.ToVector3() + start.equipOffset;
			line = scene.GetSupercoverLine(chestHeight, target.ToVector3());
			collision = CalculateImpactPoint();
		}

		// Return collision data where ray line hits a model in a tile
		private RayCollision CalculateImpactPoint()
		{
			RayCollision result = new RayCollision();
			impactData = new TargetImpactData(new Vector3(float.PositiveInfinity), Position.Negative, (Wall)(3));

			foreach (Position pos in line) {
				// I'll let Bill Gates optimize this mess
				if (scene.IsTileOccupied(pos) || scene.IsTileBlocking(pos)) {
					Vector3 chestHeight = start.position.ToVector3() + start.equipOffset;
					Ray ray = new Ray(chestHeight, Vector3.Normalize(target.ToVector3() - chestHeight));
					Tile tile = scene.GetTile(pos);

					/* Test collisiion with all walls, all objects, and the unit itself
					 * then sort by distance along line to find first hit. inefficient,
					 * and there's a reason xcom used voxels but i have an i7 dammit
					 */

					if (tile.HasWall(Wall.North)) {
						BoundingBox box = new(-0.5f * Vector3.One, 0.5f * Vector3.One); // Constant for our cube mesh
						box.min = Vector3Transform(box.min, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformNorth);
						box.max = Vector3Transform(box.max, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformNorth);
						result = Raylib.GetRayCollisionBox(ray, box);
						// If collision is closer than current one
						if (result.hit && Vector3.DistanceSquared(chestHeight, result.point) < Vector3.DistanceSquared(chestHeight, impactData.Point)) {
							impactData.Point = result.point;
							impactData.Tile = pos;
							impactData.HitType = Wall.North;
						}
					}

					if (tile.HasWall(Wall.West)) {
						BoundingBox box = new(-0.5f * Vector3.One, 0.5f * Vector3.One);
						box.min = Vector3Transform(box.min, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
						box.max = Vector3Transform(box.max, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
						result = Raylib.GetRayCollisionBox(ray, box);
						if (result.hit && Vector3.DistanceSquared(chestHeight, result.point) < Vector3.DistanceSquared(chestHeight, impactData.Point)) {
							impactData.Point = result.point;
							impactData.Tile = pos;
							impactData.HitType = Wall.West;
						}
					}

					if (tile.HasThing()) {
						// Moshi Moshi
					}
				}
			}
			return result;
		}

		// As opposed to think gravity
		// Just animate the equiped item firing and projectile travelling
		private void ThinkStraight(float dt)
		{
			Vector3 final = collision.hit ? collision.point : target.ToVector3();
			float projectileSpeed = 0.1f;
			// Absolutely horrendous
			int endPhase = (int)System.MathF.Ceiling(Vector3.Distance(start.position.ToVector3(), final) / projectileSpeed);

			if (start.phase == 0) {
				Vector3 rod = final - start.position.ToVector3() - start.equipOffset;
				float angleV = -MathF.Atan2(rod.Z, rod.X);

				actionEffect = new(item.actionEffect, 8,
					start.position.ToVector3() + start.equipOffset,
					Vector3.One,
					Vector3.UnitY * angleV);
				scene.AddParticleEffect(actionEffect);
			} else if (start.phase < endPhase) {
				actionEffect.position = Vector3.Lerp(start.position.ToVector3() + start.equipOffset, final, (float)start.phase / (float)endPhase);
			} else if (start.phase == endPhase) {
				scene.RemoveParticleEffect(actionEffect);
			} else if (start.phase >= endPhase) {
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
			//actionEffect.Done();
			if (collision.hit) {
				nextAction = new ActionTargetImpact(scene, item, impactData);
			} else {
				nextAction = null;
			}
		}
	}
}
