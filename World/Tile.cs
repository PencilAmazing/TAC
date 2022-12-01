namespace TAC.World
{
	[System.Flags]
	public enum Wall : byte
	{
		North = 1 << 0,
		West = 1 << 1,
		FlipNorth = 1 << 2,
		FlipWest = 1 << 3
	};

	public class Brush
	{
		public int[] faces;

		// Just syntax sugar
		public int top { get => faces[0]; set => faces[0] = value; }
		public int front { get => faces[1]; set => faces[1] = value; }
		public int bottom { get => faces[2]; set => faces[2] = value; }
		public int back { get => faces[3]; set => faces[3] = value; }
		public int right { get => faces[4]; set => faces[4] = value; }
		public int left { get => faces[5]; set => faces[5] = value; }

		public Brush() => faces = new int[] { 0, 1, 2, 3, 4, 5 };

		public Brush(int top = 6, int front = 1, int bottom = 2, int back = 3, int right = 4, int left = 5)
		{
			this.faces = new int[6];
			this.top = top;
			this.front = front;
			this.bottom = bottom;
			this.back = back;
			this.right = right;
			this.left = left;
		}

		public Brush(int[] faces)
		{
			this.faces = faces;
		}

		public static readonly Brush nullBrush = null;
		// Use the resource cache
		//public static readonly Brush One = new Brush(1, 1, 1, 1, 1, 1);
		//public static readonly Brush Random = new Brush(1, 2, 3, 4, 5, 6);
	};

	// Maybe walls should be in a separate array?
	public struct Tile
	{
		// Zero means null
		public int type;
		public int North;
		public int West;
		public int thing;

		/// <summary>
		/// At this point it's just wall data
		/// </summary>
		public byte walls;
		public Unit unit;

		public Tile(int type = 0, byte walls = 0)
		{
			this.type = type;
			this.walls = 0;
			this.North = this.West = 0;
			this.unit = null;
			this.thing = 0;
		}

		public Tile(int type, int north, int west, int thing)
		{
			this.type = type;
			this.North = north;
			this.West = west;
			this.thing = thing;
			this.walls = 0;
			if (north > 0)
				this.walls |= (byte)Wall.North;
			if (west > 0)
				this.walls |= (byte)Wall.West;
			this.unit = null;
		}

		public bool HasWall(Wall wall) => (walls & (byte)wall) != 0;

		// TODO Implement things
		public bool HasThing() => false;

		public static bool operator ==(Tile l, Tile r)
		{
			return l.type == r.type &&
				   l.North == r.North &&
				   l.West == r.West &&
				   l.walls == r.walls &&
				   l.unit == r.unit;
		}
		public static bool operator !=(Tile l, Tile r) => !(l == r);

		public static readonly Tile nullTile = new Tile(-1);
	}
}
