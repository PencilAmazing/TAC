using Raylib_cs;
using System;
using System.Numerics;

// This is why you don't make your own engine
// Wish I was doing XNA instead lmao
namespace TAC.Editor
{
	// Could be an interface but idgaf
	public class Resource
	{
		public readonly string assetname;
		protected Resource(string assetname)
		{
			this.assetname = assetname;
		}

		public override string ToString() => assetname;

	}

	public class Texture : Resource
	{
		public Texture2D texture;

		public Texture(string assetname, Texture2D texture)
			: base(assetname)
		{
			this.texture = texture;
		}
	}

	public class Model : Resource
	{
		// HACK model has built in materials, get rid of them
		// but we need to stick to model because raylib stores skeletal
		// animation inside these structs
		public Raylib_cs.Model model;

		public Model(string assetname, Raylib_cs.Model model)
			: base(assetname)
		{
			this.model = model;
		}

		// not static because https://github.com/raysan5/raylib/issues/2863
		// and we can't search animation for bones directly
		/// <summary>
		/// Return location of bone in local space. Applies scale and rotation for blender imported IQM
		/// </summary>
		/// <param name="animation"></param>
		/// <param name="frameIndex"></param>
		/// <param name="boneName"></param>
		/// <returns></returns>
		public unsafe Vector3 GetAnimationBoneLocation(Raylib_cs.ModelAnimation animation, int frameIndex, string boneName)
		{
			if (animation.boneCount == 0 || model.boneCount == 0 ||
				frameIndex >= animation.frameCount) return Vector3.Zero;
			int boneIndex = -1;
			// Find bone index
			for (int i = 0; i < model.boneCount; i++) {
				BoneInfo bone = model.bones[i];
				// Hardcoded 32 byte length
				string nameArray = new string(bone.name);
				if (nameArray == boneName) {
					boneIndex = i;
					break;
				}
			}
			if (boneIndex == -1) return Vector3.Zero;

			Transform boneTransform = animation.framePoses[frameIndex][boneIndex];
			boneTransform.translation *= 0.02f;
			boneTransform.translation = Raymath.Vector3RotateByAxisAngle(boneTransform.translation, Vector3.UnitX, -MathF.PI / 2.0f);
			//var rot = new Quaternion(boneTransform.rotation.X, boneTransform.rotation.Y, boneTransform.rotation.Z, boneTransform.rotation.W);
			//boneTransform.translation = Raymath.Vector3RotateByQuaternion(boneTransform.translation, rot);
			return boneTransform.translation;
		}
	}

	public class ModelAnimation : Resource
	{
		public Raylib_cs.ModelAnimation animation;

		public ModelAnimation(string assetname, Raylib_cs.ModelAnimation animation)
			: base(assetname)
		{
			this.animation = animation;
		}
	}
}
