using System.Numerics;
using TAC.World;
using Raylib_cs;
using TAC.Editor;
using static Raylib_cs.Raymath;
using System;
using System.Reflection;

namespace TAC.Logic
{

	public class ActionSelectTarget : Action
	{
		private Item item;
		public Unit unit;
		public Position target;
		public Position[] line;

		// TODO get rid of this, cache it all in our own data structure
		public RayCollision collision;
		public TargetImpactData impactData;

		private ParticleEffect actionEffect;

		public ActionSelectTarget(Scene scene, Unit unit, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.unit = unit;
			this.phase = 0;
			this.target = target;
			Vector3 chestHeight = unit.position.ToVector3() + unit.equipOffset;
			line = scene.GetSupercoverLine(chestHeight, target.ToVector3());
			collision = CalculateImpactPoint();
		}

		// Return collision data where ray line hits a model in a tile
		private RayCollision CalculateImpactPoint()
		{
			RayCollision result = new RayCollision();
			impactData = new TargetImpactData(target.ToVector3(), target, 0);

			foreach (Position pos in line) {
				// I'll let Bill Gates optimize this mess
				if (pos != unit.position && (scene.IsTileOccupied(pos) || scene.IsTileBlocking(pos))) {
					Vector3 chestHeight = unit.position.ToVector3() + unit.equipOffset;
					Tile tile = scene.GetTile(pos);

					Ray ray;
					if (tile.HasUnit())
						ray = new Ray(chestHeight, Vector3.Normalize(target.ToVector3() + tile.unit.equipOffset - chestHeight));
					else
						ray = new Ray(chestHeight, Vector3.Normalize(target.ToVector3() - chestHeight));

					/* Test collisiion with all walls, all objects, and the unit itself
					 * then sort by distance along line to find first hit. inefficient,
					 * and there's a reason xcom used voxels but i have an i7 dammit
					 * FIXME simulate bullet width by making up a whole damn algorithm
					 * or read a paper or something you lazy code monkey
					 */

					if (tile.HasWall(Wall.North)) {
						BoundingBox box = Raylib.GetMeshBoundingBox(scene.cache.cube); // Constant for our cube mesh
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
						BoundingBox box = Raylib.GetMeshBoundingBox(scene.cache.cube); // Constant for our cube mesh
						box.min = Vector3Transform(box.min, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
						box.max = Vector3Transform(box.max, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
						result = Raylib.GetRayCollisionBox(ray, box);
						if (result.hit && Vector3.DistanceSquared(chestHeight, result.point) < Vector3.DistanceSquared(chestHeight, impactData.Point)) {
							impactData.Point = result.point;
							impactData.Tile = pos;
							impactData.HitType = Wall.West;
						}
					}

					if (tile.HasUnit()) {
						BoundingBox box = tile.unit.GetUnitBoundingBox();
						result = Raylib.GetRayCollisionBox(ray, box);
						if (result.hit && Vector3.DistanceSquared(chestHeight, result.point) < Vector3.DistanceSquared(chestHeight, impactData.Point)) {
							impactData.Point = result.point;
							impactData.Tile = pos;
							impactData.HitType = (Wall)3;
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
			int endPhase = (int)MathF.Ceiling(Vector3.Distance(unit.position.ToVector3(), final) / projectileSpeed);

			if (phase == 0) {
				Vector3 rod = final - unit.position.ToVector3() - unit.equipOffset;
				float angleV = -MathF.Atan2(rod.Z, rod.X);

				actionEffect = new(item.actionEffect, 8,
					unit.position.ToVector3() + unit.equipOffset,
					Vector3.One, Vector3.UnitY * angleV);
				scene.AddParticleEffect(actionEffect);
			} else if (phase < endPhase) {
				actionEffect.position = Vector3.Lerp(unit.position.ToVector3() + unit.equipOffset, final, phase / (float)endPhase);
			} else if (phase == endPhase) {
				scene.RemoveParticleEffect(actionEffect);
			} else if (phase >= endPhase) {
				Done();
			}

			phase += 1;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);

			UnitDirection targetDir = Pathfinding.GetDirection(unit.position, target);
			if (unit.direction != targetDir) {
				nextAction = new ActionTurnUnit(scene, unit, Pathfinding.GetDirection(unit.position, target));
				nextAction.SetNextAction(new ActionPassthrough(scene, this)); // Return to this once done
				base.Done(); // Not actually done.
			} else if (item.projectileType == ProjectileType.Straight) {
				ThinkStraight(deltaTime);
			} else if (item.projectileType == ProjectileType.Gravity) {
				Done();
			} else {
				Done();
			}
		}

		public override void Done()
		{
			// Don't set isDone flag just yet
			// TODO replace isDone flag with a ActionIsDone maybe>
			base.Done();

			if (collision.hit) {
				nextAction = new ActionTargetImpact(scene, item, impactData);
			};
		}
	}
}
