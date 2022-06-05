using System.Collections.Generic;

namespace TAC.World
{
	public enum Wall : byte
	{
		North = 1 << 0,
		West = 1 << 1,
		FlipNorth = 1 << 2,
		FlipWest = 1 << 3
	};

	public class Brush
	{
		public int top;
		public int front;
		public int bottom;
		public int back;
		public int right;
		public int left;

		public Brush()
		{
			top = 1;
			front = 2;
			bottom = 3;
			back = 4;
			right = 5;
			left = 6;
		}

		public Brush(int top = 6, int front = 1, int bottom = 2, int back = 3, int right = 4, int left = 5)
		{
			this.top = top;
			this.front = front;
			this.bottom = bottom;
			this.back = back;
			this.right = right;
			this.left = left;
		}

		/// <summary>
		/// Just syntax sugar
		/// </summary>
		public static readonly Brush nullBrush = null;
		public static readonly Brush One = new Brush(1, 1, 1, 1, 1, 1);
		public static readonly Brush Random = new Brush(1, 2, 3, 4, 5, 6);
	};

	// Maybe walls should be in a separate array?
	public struct Tile
	{
		public int type;
		public Brush North;
		public Brush West;

		/// <summary>
		/// At this point it's just wall data
		/// </summary>
		public byte walls;
		public Unit unit;

		public Tile(int type = 0, byte walls = 0)
		{
			this.type = type;
			this.walls = 0;
			this.North = this.West = null;
			this.unit = null;
		}

		public bool HasWall(Wall wall) => (walls & (int)wall) != 0;

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
