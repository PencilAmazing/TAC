﻿using System;
using System.Numerics;

namespace TAC.World
{
	public struct Position
	{
		public int x;
		public int y;
		public int z;

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

		public Vector3 ToVector3()
		{
			return new Vector3(x, y, z);
		}

		public static Position Abs(Position value)
		{
			return new Position(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z));
		}

		public override string ToString()
		{
			return x.ToString() + ',' + y.ToString() + ',' + z.ToString();
		}

		public override bool Equals(object obj)
		{
			return obj is Position position &&
				   x == position.x &&
				   y == position.y &&
				   z == position.z;
		}

		// Easy enough...
		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public static Position operator +(Position pos) => pos;
		public static Position operator -(Position pos) => new(-pos.x, -pos.y, -pos.z);
		public static Position operator +(Position l, Position r) => new(l.x + r.x, l.y + r.y, l.z + r.z);
		public static Position operator -(Position l, Position r) => l + (-r);

		public static bool operator ==(Position l, Position r) => l.x == r.x && l.y == r.y && l.z == r.z;
		public static bool operator !=(Position l, Position r) => !(l == r);
	}

}
