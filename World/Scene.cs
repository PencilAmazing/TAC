using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using TAC.Render;
using TAC.Editor;

namespace TAC.World
{
	public class Scene
	{
		public List<Unit> units;

		public Floor floor;

		public ResourceCache cache;
		public Renderer renderer { get; }
		public Position size { get; }

		private Stack<DebugText> debugStack;

		public Scene(Position size, Renderer renderer, ResourceCache cache)
		{
			this.size = size;
			this.cache = cache;
			this.renderer = renderer;
			floor = new Floor(size.x, size.y);
			floor.CreateTexture(cache);

			debugStack = new Stack<DebugText>();
		}

		public virtual void Think(float deltaTime) { return; }

		public virtual void Draw(Camera3D camera)
		{
			renderer.DrawSkybox(camera, cache);
			renderer.DrawFloor(camera, floor, cache);

			// Draw wall
			for (int i = 0; i < floor.length; i++) {
				for (int j = 0; j < floor.width; j++) {
					Tile tile = floor.GetTile(i, j);
					if (tile.North > 0)
						renderer.DrawWall(camera, new Vector3(i, 0, j), false, tile.HasWall(Wall.FlipNorth), cache.brushes[tile.North], cache);
					if (tile.West > 0)
						renderer.DrawWall(camera, new Vector3(i, 0, j), true, tile.HasWall(Wall.FlipWest), cache.brushes[tile.West], cache);
				}
			}
		}

		public void PushDebugText(DebugText text) => debugStack.Push(text);

		public void DrawDebug()
		{
			while (debugStack.Count > 0) {
				DebugText text = debugStack.Pop();
				Raylib.DrawText(text.text, text.posx, text.posy, text.fontSize, text.color);
			}
			debugStack.TrimExcess();
		}

		public virtual void DrawDebug3D(Camera3D camera) { return; }
		public virtual void DrawUI() { return; }

		public Model GetFloorQuad()
		{
			return floor.GetQuad();
		}

		public Tile GetTile(Position pos)
		{
			return floor.GetTile(pos.x, pos.z);
		}

		public bool IsTileOccupied(Position pos)
		{
			Tile tile = floor.GetTile(pos.x, pos.z);
			return tile == Tile.nullTile || tile.unit != null; // Or tile has object
		}

		/// <summary>
		/// True if adjacent direction is blocked
		/// </summary>
		/// <param name="dir">Direction to walk towards</param>
		public bool TestDirection(Position pos, UnitDirection dir)
		{
			int x = pos.x;
			int z = pos.z;
			// Can't be arsed to inline
			bool north = floor.GetTile(x, z).North > -1;
			bool west = floor.GetTile(x, z).West > -1;
			bool south = floor.GetTile(x, z + 1).North > -1;
			bool east = floor.GetTile(x + 1, z).West > -1;
			return dir switch
			{
				UnitDirection.North => north,
				UnitDirection.West => west,
				UnitDirection.South => south,
				UnitDirection.East => east,
				UnitDirection.NorthEast => north || east,
				UnitDirection.NorthWest => north || west,
				UnitDirection.SouthEast => south || east,
				UnitDirection.SouthWest => south || west,
				_ => false
			};
		}
	}
}
