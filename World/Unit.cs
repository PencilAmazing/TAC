using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;

namespace TAC.World
{
	public enum UnitDirection
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
	};

	public class Unit
	{
		public static Vector3[] VectorDirections = {
				Vector3.Normalize(Vector3.UnitZ),
				Vector3.Normalize(Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ),
				Vector3.Normalize(-Vector3.UnitZ-Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitZ-Vector3.UnitX)
		};

		public string name;
		public int type { get; }
		public int faction { get; }
		public int time { get; }
		public List<Item> inventory;

		public Position position;
		public UnitDirection direction;

		// Chest height I guess
		public Vector3 equipOffset = new Vector3(0, 0.6f, 0);

		// General purpose counter
		public int phase;

		public Unit(int type, Position position, string name, UnitDirection direction = UnitDirection.North, List<Item> inventory = null)
		{
			this.type = type;
			this.position = position;
			this.direction = direction;
			this.name = name;
			this.phase = 0;
			this.inventory = inventory == null ? new List<Item>() : inventory;
		}

		public void Think(float deltaTime)
		{
		}

		/// <summary>
		/// Return true if possible
		/// </summary>
		public bool AddToInventory(Item item)
		{
			if(inventory.Count <= 2) {
				inventory.Add(item);
				return true;
			}
			return false;
		}
	}
}
