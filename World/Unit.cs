using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using TAC.Editor;
using TAC.Logic;

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

	/// <summary>
	/// Gameplay represenation of a unit that moves and interacts with a scene.<br></br>
	/// Not for database representation, use something else.
	/// All values here are volatile and constantly changing, owned by the game scene
	/// </summary>

	public class Unit
	{
		public static readonly Vector3[] VectorDirections = {
				Vector3.Normalize(Vector3.UnitZ),
				Vector3.Normalize(Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ),
				Vector3.Normalize(-Vector3.UnitZ-Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitZ-Vector3.UnitX)
		};

		public readonly UnitTemplate Type;

		public string Name;
		/// <summary>
		/// Index of team in scene team list
		/// TODO replace with Team reference
		/// </summary>
		public int TeamID;
		public int TimeUnits;
		public int Health;
		public List<Item> inventory;

		public Position position;
		public UnitDirection direction;

		// Chest height I guess
		public Vector3 equipOffset = new Vector3(0, 0.6f, 0);

		// General purpose counter
		public int phase;

		public UnitAIModule UnitAI;
		public bool isDone;

		public Unit(UnitTemplate type, Position position, string name, UnitDirection direction = UnitDirection.North, List<Item> inventory = null)
		{
			this.Type = type;
			this.position = position;
			this.direction = direction;
			this.Name = name;
			this.phase = 0;
			this.TimeUnits = type.TimeUnits;
			this.Health = type.Health;
			this.inventory = inventory == null ? new List<Item>() : inventory;

			UnitAI = null;
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

		public void Reset()
		{
			if(UnitAI != null) UnitAI.Reset();
			TimeUnits = Type.TimeUnits;
			isDone = false;
		}
	}
}
