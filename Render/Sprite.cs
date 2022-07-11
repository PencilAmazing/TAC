using System.Numerics;

namespace TAC.Render
{
	// Data only class, no functionality whatsoever
	public class Sprite
	{
		public int id; // ID of effect texture stored in misc cache
		public int numFrames; // stage = phase/timing % numFrames

		public int frameWidth; // Size of single frame
		public int frameHeight;

		public Sprite(int id, int numFrames, int frameWidth, int frameHeight)
		{
			this.id = id;
			this.numFrames = numFrames;
			this.frameHeight = frameHeight;
			this.frameWidth = frameWidth;
		}

		/// <summary>
		/// stage = phase/interval % numFrames
		/// </summary>
		public Raylib_cs.Rectangle GetRectangle(int stage)
		{
			return new Raylib_cs.Rectangle(stage * frameWidth, 0, frameWidth, frameHeight);
		}
	}
}
