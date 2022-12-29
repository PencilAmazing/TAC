using TAC.Logic;
using TAC.World;
using static TAC.Inner.PlayerController;

namespace TAC.Inner
{
	namespace ControllerStates
	{
		public struct ControlEditState
		{
			public Brush selectedBrush;
			public bool FlipBrush;

			public ControlEditState(Brush selectedBrush, bool FlipBrush)
			{
				this.selectedBrush = selectedBrush;
				this.FlipBrush = FlipBrush;
			}
		}

		public struct ControlGameState
		{
			public Unit SelectedUnit;
			public Team SelectedTeam;
			public GameSelection Mode;

			public ControlGameState(Unit selectedUnit = null, Team selectedTeam = null, GameSelection mode = GameSelection.SelectUnit)
			{
				this.SelectedUnit = selectedUnit;
				this.SelectedTeam = selectedTeam;
				this.Mode = mode;
			}
		}
	}
}
