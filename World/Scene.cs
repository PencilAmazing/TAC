using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using TAC.Render;

namespace TAC.World
{
	class Scene
	{
		public Floor floor;

		protected ResourceCache cache;
		public Renderer renderer { get; }

		private Stack<DebugText> debugStack;

		public Scene(Position size, Renderer renderer, ResourceCache cache)
		{
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
					if (tile.North == Brush.nullBrush && tile.West == Brush.nullBrush) continue;
					if (tile.North != Brush.nullBrush)
						renderer.DrawWall(camera, new Vector3(i, 0, j), false, tile.HasWall(Wall.FlipNorth), tile.North, cache);
					if (tile.West != Brush.nullBrush)
						renderer.DrawWall(camera, new Vector3(i, 0, j), true, tile.HasWall(Wall.FlipWest), tile.West, cache);
				}
			}
		}

		public void PushDebugText(DebugText text)
		{
			debugStack.Push(text);
		}

		public void DrawDebug()
		{
			while (debugStack.Count > 0) {
				DebugText text = debugStack.Pop();
				Raylib.DrawText(text.text, text.posx, text.posy, text.fontSize, text.color);
			}
			debugStack.TrimExcess();
		}

		public virtual void DrawDebug3D(Camera3D camera) { return; }

		public Model GetFloorQuad()
		{
			return floor.GetQuad();
		}

		public Tile GetTile(Position pos)
		{
			return floor.GetTile(pos.x, pos.z);
		}
	}
}
