using Raylib_cs;
using System.Numerics;

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

		public int type { get; }
		public Position position;
		public UnitDirection direction;
		public string name;
		// General purpose counter
		public int phase;

		public Unit(int type, Position position, string name, UnitDirection direction = UnitDirection.North)
		{
			this.type = type;
			this.position = position;
			this.direction = direction;
			this.name = name;
			this.phase = 0;
		}

		public void Think(float deltaTime)
		{
		}
	}
}
