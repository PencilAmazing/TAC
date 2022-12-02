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
		/// <summary> World position </summary>
		public Vector3 position;
		/// <summary> World scale </summary>
		public Vector3 scale;
		/// <summary> World rotate </summary>
		public Vector3 rotate;

		public ParticleEffect(Sprite actionEffectMaterial, int interval, Vector3 position, Vector3 scale, Vector3 rotate)
		{
			this.sprite = actionEffectMaterial;
			this.phase = 0;
			this.interval = interval;

			this.position = position;
			this.scale = scale;
			this.rotate = rotate;
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
