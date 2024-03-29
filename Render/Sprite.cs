﻿using TAC.Editor;

namespace TAC.Render
{
	/// <summary>
	/// Data only class
	/// All sprites must scroll horizontally only
	/// </summary>
	public class Sprite
	{
		public Texture texture; // ID of effect texture stored in misc cache
		public int numFrames; // stage = phase/timing % numFrames

		public int frameWidth; // Size of single frame
		public int frameHeight;

		public Sprite(Texture texture, int numFrames, int frameWidth, int frameHeight)
		{
			this.texture = texture;
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
