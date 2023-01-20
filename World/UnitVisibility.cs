using System.Collections.Generic;

// https://www.albertford.com/shadowcasting/
namespace TAC.World
{
	class Fraction
	{
		public float numerator;
		public float denominator;

		public Fraction(float numerator, float denominator)
		{
			this.numerator = numerator;
			this.denominator = denominator;
		}

		public Fraction(Fraction fraction)
		{
			numerator = fraction.numerator;
			denominator = fraction.denominator;
		}

		public override bool Equals(object obj)
		{
			Fraction other = obj as Fraction;
			return (numerator == other.numerator && denominator == other.denominator);
		}

		public static bool operator ==(Fraction f1, Fraction f2) => f1.Equals(f2);

		public static bool operator !=(Fraction f1, Fraction f2) => !(f1 == f2);

		public static Fraction operator *(Fraction f1, float f2)
		{
			var f3 = new Fraction(f1);
			f3.numerator *= f2;
			return f3;
		}

		public static Fraction operator /(Fraction f1, float f2)
		{
			var f3 = new Fraction(f1);
			f3.numerator /= f2;
			f3.denominator /= f2;
			return f3;
		}

		public static Fraction operator +(Fraction f1, float f2)
		{
			var f3 = new Fraction(f1.numerator, f1.denominator);
			f3.numerator += f2 * f3.denominator;
			return f3;
		}

		public static Fraction operator -(Fraction f1, float f2)
		{
			var f3 = new Fraction(f1.numerator, f1.denominator);
			f3.numerator -= f2 * f3.denominator;
			return f3;
		}

		public static Fraction operator +(Fraction f1, Fraction f2)
		{
			var a = f1.denominator > f2.denominator ? f1 : f2;
			var b = f1.denominator < f2.denominator ? f1 : f2;
			var difference = a.denominator / b.denominator;

			return new Fraction(a.numerator + (b.numerator * difference), a.denominator);
		}

		public static Fraction operator -(Fraction f1, Fraction f2)
		{
			var a = f1.denominator > f2.denominator ? f1 : f2;
			var b = f1.denominator < f2.denominator ? f1 : f2;
			var difference = a.denominator / b.denominator;

			return new Fraction(a.numerator - (b.numerator * difference), a.denominator);
		}

		public static bool operator >(Fraction f1, Fraction f2) => (f1.numerator / f1.denominator) > (f2.numerator / f2.denominator);

		public static bool operator <(Fraction f1, Fraction f2) => (f1.numerator / f1.denominator) < (f2.numerator / f2.denominator);

		public static bool operator <=(Fraction f1, Fraction f2) => (f1.numerator / f1.denominator) <= (f2.numerator / f2.denominator);

		public static bool operator >=(Fraction f1, Fraction f2) => (f1.numerator / f1.denominator) >= (f2.numerator / f2.denominator);

		public static bool operator >(Fraction f1, float f2) => (f1.numerator / f1.denominator) > f2;

		public static bool operator <(Fraction f1, float f2) => (f1.numerator / f1.denominator) < f2;

		public static bool operator <=(Fraction f1, float f2) => (f1.numerator / f1.denominator) <= f2;

		public static bool operator >=(Fraction f1, float f2) => (f1.numerator / f1.denominator) >= f2;

		public float ToFloat() => numerator / denominator; // Fuck you
		public override string ToString() => numerator + "/" + denominator;
	}

	class Quadrant
	{
		Position origin;
		UnitDirection direction;

		public Quadrant(UnitDirection direction, Position origin)
		{
			this.direction = direction;
			this.origin = origin;
		}

		public Position Transform(Position position)
		{
			int row = position.x;
			int column = position.z;
			if (direction == UnitDirection.South) return origin + new Position(column, 0, -row);
			if (direction == UnitDirection.North) return origin + new Position(column, 0, row);
			if (direction == UnitDirection.East) return origin + new Position(row, 0, column);
			if (direction == UnitDirection.West) return origin + new Position(-row, 0, column);
			throw new System.Exception("Fuck you");
		}
	}

	class Row
	{
		private static int round_up(float x) => (int)(x + 0.5);
		private static int round_down(float x) => (int)(x - 0.5);

		public int depth;
		public Fraction start_slope, end_slope;

		public Row(int depth, Fraction start_slope, Fraction end_slope)
		{
			this.depth = depth;
			this.start_slope = start_slope;
			this.end_slope = end_slope;
		}

		public IEnumerable<Position> Tiles()
		{
			int min_col = round_up((start_slope * depth).ToFloat());
			int max_col = round_down((end_slope * depth).ToFloat());

			for (int col = min_col; col < max_col + 1; col++) {
				yield return new Position(depth, 0, col);
			}
		}

		public Row Next() => new Row(depth + 1, start_slope, end_slope);
	}

	public partial class Scene
	{
		// http://www.adammil.net/blog/v125_roguelike_vision_algorithms.html#shadowcode
		// Actually shadowcasting
		// TODO move this to tilespace
		// TODO implement diagonal views by modifying quadrant class
		// TODO implement z direction by hacking in another slope pair for z axis
		public List<Position> GetUnitVisibleTiles(Position start, UnitDirection dir)
		{
			//Just for now lfmao
			if (dir != UnitDirection.North && dir != UnitDirection.South && dir != UnitDirection.East && dir != UnitDirection.West) return null;
			Position[][] boundarydefs = new Position[8][];
			// Start, End, Delta
			boundarydefs[(int)UnitDirection.North] = new Position[3] { new(-4, 0, -4), new(4, 0, -4), new(1, 0, 0) };
			boundarydefs[(int)UnitDirection.East] = new Position[3] { new(4, 0, -4), new(4, 0, 4), new(0, 0, 1) };
			boundarydefs[(int)UnitDirection.South] = new Position[3] { new(-4, 0, 4), new(4, 0, 4), new(1, 0, 0) };
			boundarydefs[(int)UnitDirection.West] = new Position[3] { new(-4, 0, -4), new(-4, 0, 4), new(0, 0, 1) };

			/*
			 I swear it looks better when it actually works
			 South:
				@
			   ***
			  *****
			 *******

			 East:
				   *
				  **
				 ***
				@***
				 ***
				  **
				   *

			SouthEast:
				@*****
				*****
				****
				***
				**
				*
			  Basically find a 'boundary' and line cast from player to every cell in line
			 */

			//Position boundarystart = start + boundarydefs[(int)dir][0];
			//Position boundaryend = start = boundarydefs[(int)dir][1];
			//Position diff = boundarydefs[(int)dir][2];

			// 90 degrees FOV
			List<Position> visible = new();

			// Assist functions
			void SetVisible(Position pos) => visible.Add(pos);
			bool IsWall(Tile tile) => tile.IsTileInvalid() ? false : tile.IsTileBlocking();
			bool IsFloor(Tile tile) => tile.IsTileInvalid() ? false : !tile.IsTileBlocking();
			bool IsSymmetric(Row row, Tile tile) => true; // IDK dude

			Fraction slope(Position tile) => new Fraction(2 * tile.z - 1, 2 * tile.x);

			Quadrant quad = new Quadrant(dir, start);

			void Scan(Row row)
			{
				Stack<Row> rows = new();
				rows.Push(row);
				while (rows.Count > 0) {
					row = rows.Pop();
					Tile prev_tile = Tile.nullTile;
					foreach (Position pos in row.Tiles()) {
						Tile tile = GetTile(quad.Transform(pos));
						if (IsWall(tile) || IsSymmetric(row, tile)) {
							SetVisible(quad.Transform(pos));
						}
						if (IsWall(prev_tile) && IsFloor(tile)) {
							row.start_slope = slope(pos);
						}
						if (IsFloor(prev_tile) && IsWall(tile)) {
							Row next_row = row.Next();
							next_row.end_slope = slope(pos);
							rows.Push(next_row);
						}
						prev_tile = tile;
					}
					if (IsFloor(prev_tile)) {
						rows.Push(row.Next());
					}
				}
			}

			Row first_row = new Row(1, new Fraction(-1, 1), new Fraction(1, 1));
			Scan(first_row);

			return visible;
		}
	}
}
