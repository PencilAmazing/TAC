using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using TAC.Editor;

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

		public string Name;
		// FIXME Make this of type UnitType or UnitTemplate maybe
		public readonly UnitTemplate Type;
		public int Faction { get; }
		public int TimeUnits;
		public int Health;
		public List<Item> inventory;

		public Position position;
		public UnitDirection direction;

		// Chest height I guess
		public Vector3 equipOffset = new Vector3(0, 0.6f, 0);

		// General purpose counter
		public int phase;

		public Unit(UnitTemplate type, Position position, string name, UnitDirection direction = UnitDirection.North, List<Item> inventory = null)
		{
			this.Type = type;
			this.position = position;
			this.direction = direction;
			this.Name = name;
			this.phase = 0;
			this.TimeUnits = 100;
			this.Health = 80;
			this.inventory = inventory == null ? new List<Item>() : inventory;
		}

		public void Think(float deltaTime)
		{
		}

		/// <summary>
		/// Return false if unit should be removed from game
		/// </summary>
		public bool IsAlive()
		{
			return Health > 0;
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

		public BoundingBox GetUnitBoundingBox()
		{
			BoundingBox box = new(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 2, 0.5f));
			box.min = Raymath.Vector3Transform(box.min, Raymath.MatrixTranslate(position.x, position.y, position.z));
			box.max = Raymath.Vector3Transform(box.max, Raymath.MatrixTranslate(position.x, position.y, position.z));
			return box;
		}

	}
}
