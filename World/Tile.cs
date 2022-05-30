using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAC.World
{
	public enum Wall : byte
	{
		North = 1 << 0,
		East = 1 << 1,
		South = 1 << 2,
		West = 1 << 3
	};

	public struct Tile
	{
		public int type;
		public byte walls;
		public Unit unit;

		public Tile(int type = 0, byte walls = 0)
		{
			this.type = type;
			this.walls = walls;
			this.unit = null;
		}

		public bool HasWall(Wall wall)
		{
			return (walls & (byte)wall) != 0;
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
