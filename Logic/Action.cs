using TAC.World;

namespace TAC.Logic
{
	public abstract class Action
	{
		protected Scene scene;
		public bool isDone;
		protected Action nextAction;

		public Action(Scene scene)
		{
			this.scene = scene;
			this.isDone = false;
			this.nextAction = null;
		}

		virtual public void Think(float deltaTime) { }
		/// <summary>
		/// Cleanup effects and set this.nextAction
		/// </summary>
		virtual public void Done() { isDone = true; }
		/// <summary>
		/// Return next action for scene to execute.
		/// Can be null to indicate end of chain of events
		/// </summary>
		virtual public Action NextAction() { return nextAction; }
	}
}
