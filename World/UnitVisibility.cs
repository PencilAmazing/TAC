using System;
using System.Collections.Generic;
using TAC.Logic;

namespace TAC.World
{
	class Fraction
	{
		public int numerator;
		public int denominator;

		public Fraction(int numerator, int denominator)
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

		public override int GetHashCode() => HashCode.Combine(numerator, denominator);

		public static bool operator ==(Fraction f1, Fraction f2) => f1.Equals(f2);

		public static bool operator !=(Fraction f1, Fraction f2) => !(f1 == f2);

		public static Fraction operator *(int f2, Fraction f1) => f1 * f2;
		public static Fraction operator *(Fraction f1, int f2)
		{
			var f3 = new Fraction(f1);
			f3.numerator *= f2;
			return f3;
		}

		public static Fraction operator +(Fraction f1, int f2)
		{
			var f3 = new Fraction(f1.numerator, f1.denominator);
			f3.numerator += f2 * f3.denominator;
			return f3;
		}

		public static Fraction operator -(Fraction f1, int f2)
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
		private static int round_up(float x) => (int)MathF.Floor(x + 0.5f);
		private static int round_down(float x) => (int)MathF.Ceiling(x - 0.5f);

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
		// https://www.albertford.com/shadowcasting/
		// Actually shadowcasting
		// TODO move this to tilespace
		// TODO implement diagonal views by modifying quadrant class
		// TODO implement z direction by hacking in another slope pair for z axis
		public List<Position> GetUnitVisibleTiles(Position start, UnitDirection dir)
		{
			//Just for now lfmao
			if (dir != UnitDirection.North && dir != UnitDirection.South && dir != UnitDirection.East && dir != UnitDirection.West) return null;
			// 90 degrees FOV
			// TODO make 45 degrees
			List<Position> visible = new();

			hideCache = new List<Position>();

			Quadrant quad = new Quadrant(dir, start);
			Fraction slope(Position tile) => new Fraction(2 * tile.z - 1, 2 * tile.x);

			bool Testcallback(Position pos) => false;

			// Assist functions
			void SetVisible(Position pos)
			{
				Position transform = quad.Transform(pos);
				UnitDirection testDir = Pathfinding.GetDirectionAtan(start, transform);

				// NOTE TestDirection doesn't allow corner cutting around walls
				// which is honestly something I might want to change
				// Function was designed for movement, not sight anyways
				// Replace with something worse i mean better :^)
				if (!TestDirectionCallback(transform, testDir, Testcallback))
					visible.Add(quad.Transform(pos));
				else
					this.hideCache.Add(transform);

			}

			bool IsWall(Position tile)
			{
				if (tile == new Position(-2, -2, -2)) return false;

				// Transform tiles
				Position transform = quad.Transform(tile);
				// Then fucking check if that shit's valid
				if (IsTileInvalid(transform)) return false;

				UnitDirection testDir = Pathfinding.GetDirectionAtan(start, transform);
				Position StepBack = transform;
				// Relative testDir
				// Step back one tile
				StepBack += Pathfinding.GenerateVectorFromDirection(testDir);
				// StepBack is generated correctly, test is broken
				UnitDirection oppositeTest = Unit.OppositeDirections[(int)testDir];

				return TestDirectionCallback(transform, testDir, Testcallback);
			};

			bool IsFloor(Position tile)
			{
				if (tile == new Position(-2, -2, -2)) return false;
				if (IsTileInvalid(quad.Transform(tile))) return false;
				return !IsWall(tile);
			}
			bool IsSymmetric(Row row, Position tile)
			{
				int row_depth = tile.x;
				int col = tile.z;

				bool result = col >= (row.start_slope.ToFloat() * row.depth) &&
					   col <= (row.end_slope.ToFloat() * row.depth);
				return result;
			};

			void Scan(Row row)
			{
				Stack<Row> rows = new();
				rows.Push(row);
				while (rows.Count > 0) {
					row = rows.Pop();
					// Magic invalid number
					Position prev_tile = new Position(-2, -2, -2);
					foreach (Position tile in row.Tiles()) {
						// Checks if visile from start to tile
						if (IsWall(tile) || IsSymmetric(row, tile)) {
							SetVisible(tile); // TODO Check if visible both ways
						}
						if (IsWall(prev_tile) && IsFloor(tile)) {
							row.start_slope = slope(tile);
						}
						if (IsFloor(prev_tile) && IsWall(tile)) {
							Row next_row = row.Next();
							next_row.end_slope = slope(tile);
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
