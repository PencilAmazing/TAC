using TAC.World;

namespace TAC.Logic
{
	public abstract class Action
	{
		protected Scene scene;
		public bool isDone;

		public Action(Scene scene)
		{
			this.scene = scene;
			this.isDone = false;
		}

		virtual public void Think(float deltaTime) { }
		virtual public void Done() { isDone = true; }
	}
}
