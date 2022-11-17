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
		public Unit start;
		private Position target;
		public Position[] line;

		private ParticleEffect impactEffect;
		public RayCollision collision;

		public ActionSelectTarget(Scene scene, Unit start, Item item, Position target) : base(scene)
		{
			this.item = item;
			this.start = start;
			this.start.phase = 0;
			this.target = target;
			line = scene.GetSupercoverLine(start.position, target);
			collision = CalculateImpactPoint();
			impactEffect = new ParticleEffect(item.impactEffect, 12,
				collision.hit ? collision.point : line[line.Length - 1].ToVector3(), Vector2.One);

		}

		// Return exact coordinates where ray line hits a model in a tile
		private RayCollision CalculateImpactPoint()
		{
			RayCollision result = new RayCollision();
			foreach (Position pos in line) {
				if (scene.IsTileOccupied(pos) || scene.GetTile(pos).HasWall(Wall.North) || scene.GetTile(pos).HasWall(Wall.West)) {
					Vector3 chestHeight = start.position.ToVector3() + start.equipOffset;
					Ray ray = new Ray(chestHeight, target.ToVector3() - chestHeight);
					// impactpoint = collision trace
					Tile tile = scene.GetTile(pos);
					if (tile.HasWall(Wall.North)) {
						result = Raylib.GetRayCollisionMesh(ray, scene.cache.cube, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformNorth);
					} else if (tile.HasWall(Wall.West)) {
						result = Raylib.GetRayCollisionMesh(ray, scene.cache.cube, MatrixTranslate(pos.x, pos.y, pos.z) * scene.cache.WallTransformWest);
					} // else if(tile.HasThing()) {}
					if (result.hit) break;
				}
			}
			return result;
		}

		// As opposed to think gravity
		private void ThinkStraight(float dt)
		{
			if (start.phase == 0) {
				//scene.AddParticleEffect(actionEffect);
			} else if (start.phase / 8 < 4) {
				//actionEffect.position = Vector3.Lerp(start.position.ToVector3(), target.ToVector3(), start.phase * 4 / 8);
			} else if (start.phase / 8 == 4) {
				if (collision.hit)
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
