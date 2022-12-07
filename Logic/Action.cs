using TAC.World;

namespace TAC.Logic
{
	public abstract class Action
	{
		protected Scene scene;
		public bool isDone;
		protected Action nextAction;
		public int phase;
		// Cost of this action in time units, if needed
		// FIXME move this to per unit maybe? or a global table or something
		public int TimeCost { get; protected set; }

		public Action(Scene scene)
		{
			this.scene = scene;
			this.isDone = false;
			this.nextAction = null;
			this.phase = 0;
			this.TimeCost = 0;
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

		/// <summary>
		/// Setup this action to return control to returnAction after being done
		/// </summary>
		public void SetNextAction(Action action) => nextAction = action;
	}
}
