using System.Numerics;

namespace TAC.World
{
	public struct Position
	{
		public int x;
		public int y;
		public int z;

		public static Vector3 HalfUnitVector = Vector3.One / 2;

		public Position(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Position(Vector3 world)
		{
			world += Vector3.One / 2;
			x = (int)world.X;
			y = 0; // ehh
			z = (int)world.Z;
		}

		public Vector3 ToVector3()
		{
			return new Vector3(x, y, z);
		}

		public static Position operator +(Position pos) => pos;
		public static Position operator -(Position pos) => new(-pos.x, -pos.y, -pos.z);
		public static Position operator +(Position l, Position r) => new(l.x + r.x, l.y + r.y, l.z + r.z);
		public static Position operator -(Position l, Position r) => l + (-r);
	}

}
