using System.Numerics;
using TAC.World;
using Raylib_cs;
using TAC.Editor;
using static Raylib_cs.Raymath;

namespace TAC.Logic
{
	public class ActionSelectTarget : Action
	{
		private Item item;
		private Unit start;
		private Position target;
		public Position[] line;

		private ParticleEffect impactEffect;
		private Vector3 impactPoint;

		public ActionSelectTarget(Scene scene, Unit start, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.start = start;
			this.start.phase = 0;
			this.target = target;
			line = scene.GetSupercoverLine(start.position, target);
			impactPoint = CalculateImpactPoint();
			impactEffect = new ParticleEffect(item.impactEffect, 12, line[line.Length - 1].ToVector3(), Vector2.One);

		}

		// Return exact coordinates where ray line hits a model in a tile
		private Vector3 CalculateImpactPoint()
		{
			foreach (Position pos in line) {
				if (scene.IsTileOccupied(pos)) {
					Ray ray = new Ray(start.position.ToVector3(), target.ToVector3() - start.position.ToVector3());
					// impactpoint = collision trace
					Tile tile = scene.GetTile(pos);
					RayCollision collision = new RayCollision();
					if (tile.HasWall(Wall.North)) {
						RayCollision collide = Raylib.GetRayCollisionMesh(ray, scene.cache.cube, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformNorth);
					} else {
						RayCollision collide = Raylib.GetRayCollisionMesh(ray, scene.cache.cube, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
					}
					if (collision.hit) return collision.point;
				}
			}
			return Vector3.Zero;
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
