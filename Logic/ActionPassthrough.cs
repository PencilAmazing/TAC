using TAC.World;

namespace TAC.Logic
{
	/// <summary>
	/// Action that moves state control backwards to previous state.
	/// </summary>
	public class ActionPassthrough : Action
	{
		public ActionPassthrough(Scene scene, Action passthrough) : base(scene)
		{
			nextAction = passthrough;
		}

		public override void Think(float deltaTime)
		{
			base.Think(deltaTime);
			nextAction.isDone = false;
			nextAction.SetNextAction(null); // Clear previous action
			Done();
		}
	}
}
