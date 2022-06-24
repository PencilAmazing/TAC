using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAC.World
{
	// Make this a property of ammo, not weapon
	public enum ProjectileType { Straight, Gravity }


	public class Item
	{
		public string name;
		public int weight;
		public ProjectileType projectileType;

		public Item(string name, int weight, ProjectileType projectileType = ProjectileType.Straight)
		{
			this.name = name;
			this.weight = weight;
			this.projectileType = projectileType;
		}
	}
}
