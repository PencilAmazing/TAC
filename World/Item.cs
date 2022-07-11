using TAC.Render;

namespace TAC.World
{
	// Make this a property of ammo, not weapon
	public enum ProjectileType { Straight, Gravity }


	public class Item
	{
		public string name;
		public int weight;
		public ProjectileType projectileType;
		public Sprite impactEffect;
		public Sprite actionEffect;

		public Item(string name, int weight, Sprite impactEffect, Sprite actionEffect, ProjectileType projectileType = ProjectileType.Straight)
		{
			this.name = name;
			this.weight = weight;
			this.impactEffect = impactEffect;
			this.actionEffect = actionEffect;
			this.projectileType = projectileType;
		}
	}
}
