using System;
using System.Numerics;
using System.Text.Json.Nodes;

namespace TAC.World
{
	public struct Position
	{
		public int x;
		public int y;
		public int z;

		public static readonly Position Zero = new Position(0, 0, 0);
		public static readonly Position One = new Position(1, 1, 1);
		public static readonly Position Negative = new Position(-1, -1, -1);

		public static readonly Position PositiveX = new Position(1, 0, 0);
		public static readonly Position PositiveY = new Position(0, 1, 0);
		public static readonly Position PositiveZ = new Position(0, 0, 1);

		public Position(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Rounds float vector to nearest tile position
		/// </summary>
		public Position(Vector3 world)
		{
			x = (int)(world.X + 0.5f);
			y = (int)(world.Y / 2.0f + 0.5f);
			z = (int)(world.Z + 0.5f);
		}

		public Position(JsonArray jsonposition) : this((int)jsonposition[0], (int)jsonposition[1], (int)jsonposition[2]) { }

		public Vector3 ToVector3() => new Vector3(x, y, z);

		public static Position Abs(Position value) => new Position(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z));

		public override string ToString() => x.ToString() + ',' + y.ToString() + ',' + z.ToString();

		public override bool Equals(object obj) => obj is Position position &&
															x == position.x &&
															y == position.y &&
															z == position.z;

		// Easy enough...
		public override int GetHashCode() => HashCode.Combine(x, y, z);
		public JsonNode GetJsonNode() => new JsonArray() { x, y, z };

		public static Position operator +(Position pos) => pos;
		public static Position operator -(Position pos) => new(-pos.x, -pos.y, -pos.z);
		public static Position operator +(Position l, Position r) => new(l.x + r.x, l.y + r.y, l.z + r.z);
		public static Position operator -(Position l, Position r) => l + (-r);
		public static Position operator *(Position l, int r) => new(l.x * r, l.y * r, l.z * r);

		public static bool operator ==(Position l, Position r) => l.x == r.x && l.y == r.y && l.z == r.z;
		public static bool operator !=(Position l, Position r) => !(l == r);
	}
}
