using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAC.World
{
	// Class containing ids of wall textures
	public class Brush
	{
		public int top;
		public int front;
		public int bottom;
		public int back;
		public int right;
		public int left;
	};

	public enum Wall : byte
	{
		North = 1 << 0,
		West = 1 << 1,
		FlipNorth = 1 << 2,
		FlipWest = 1 << 3
	};


	// Maybe walls should be in a separate array?
	public struct Tile
	{
		public int type;
		public byte walls;
		public Unit unit;

		public Tile(int type = 0, byte walls = 0)
		{
			this.type = type;
			this.walls = 0;
			this.unit = null;
		}

		public bool HasWall(Wall wall)
		{
			return (walls & (int)wall) != 0;
		}

		public override bool Equals(object obj)
		{
			return obj is Tile tile &&
				   type == tile.type &&
				   EqualityComparer<Unit>.Default.Equals(unit, tile.unit);
		}

		public static readonly Tile nullTile = new Tile(-1);

		public static bool operator ==(Tile left, Tile right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Tile left, Tile right)
		{
			return !(left == right);
		}

	}
}
