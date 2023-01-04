using TAC.Logic;
using TAC.World;

namespace TAC.Inner
{
	namespace ControllerStates
	{
		public struct ControlEditState
		{
			public enum ToolType { None, Wall, Tile, Unit };

			public Brush SelectedBrush;
			public int SelectedTileIndex;
			public bool FlipBrush;
			public ToolType SelectedTool;

			public ControlEditState()
			{
				SelectedBrush = null;
				SelectedTileIndex = 0;
				FlipBrush = false;
				SelectedTool = ToolType.None;
			}
		}

		public struct ControlGameState
		{
			public enum GameSelection
			{
				SelectUnit, // Default game mode, select units to view
				SelectTarget,
				WaitAction
			}

			public Unit SelectedUnit;
			public Team SelectedTeam;
			public GameSelection Mode;

			public ControlGameState()
			{
				SelectedUnit = null;
				SelectedTeam = null;
				Mode = GameSelection.SelectUnit;
			}
		}
	}
}
