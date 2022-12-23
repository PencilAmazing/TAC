using System.Collections.Generic;
using TAC.Editor;

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

	// Wall brush
	public class Brush
	{
		public Texture[] faces;
		public string assetname;

		// Just syntax sugar
		public Texture top { get => faces[0]; set => faces[0] = value; }
		public Texture front { get => faces[1]; set => faces[1] = value; }
		public Texture bottom { get => faces[2]; set => faces[2] = value; }
		public Texture back { get => faces[3]; set => faces[3] = value; }
		public Texture right { get => faces[4]; set => faces[4] = value; }
		public Texture left { get => faces[5]; set => faces[5] = value; }

		public Brush() => faces = new Texture[6];

		public Brush(string assetname,
					 Texture top = null, Texture front = null,
					 Texture bottom = null, Texture back = null,
					 Texture right = null, Texture left = null)
		{
			this.assetname = assetname;

			this.faces = new Texture[6];
			this.top = top;
			this.front = front;
			this.bottom = bottom;
			this.back = back;
			this.right = right;
			this.left = left;
		}

		public Brush(string assetname, Texture[] faces)
		{
			this.assetname = assetname;
			this.faces = faces;
		}

		public static readonly Brush nullBrush = null;
		
		public override bool Equals(object obj)
		{
			if (!(obj is Brush brush)) return false;
			for (int i = 0; i < brush.faces.Length; i++) {
				if (this.faces[i] != brush.faces[i]) return false;
			}
			return this.assetname == brush.assetname;
		}
		// Use the resource cache
		//public static readonly Brush One = new Brush(1, 1, 1, 1, 1, 1);
		//public static readonly Brush Random = new Brush(1, 2, 3, 4, 5, 6);
	}
	// Maybe walls should be in a separate array?
	public struct Tile
	{
		// -1 means null
		// Can change 
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

		public bool HasUnit() => this.unit != null;

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

		public override bool Equals(object obj)
		{
			throw new System.NotImplementedException();
		}

		public override int GetHashCode()
		{
			throw new System.NotImplementedException();
		}
	}
}
