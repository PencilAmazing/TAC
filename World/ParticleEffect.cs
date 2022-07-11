using System.Numerics;
using TAC.Render;

namespace TAC.World
{
	// More of a scene thing than a renderer thing
	public class ParticleEffect
	{
		public Sprite sprite;

		public int phase;
		public int interval;
		public Vector3 position;
		public Vector2 scale;

		public ParticleEffect(Sprite sprite, int interval, Vector3 position, Vector2 scale)
		{
			this.sprite = sprite;
			this.phase = 0;
			this.interval = interval;
			this.position = position;
			this.scale = scale;
		}

		// Ends and removes particle effect from scene
		public void Done() => phase = -1;

		/// <summary>
		/// Return sprite stage
		/// </summary>
		public int GetStage()
		{
			// Wrap between 1 and numFrames
			return (((phase / interval)) % (sprite.numFrames));
		}
	}
}
