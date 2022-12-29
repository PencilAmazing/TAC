using System.Collections.Generic;
using TAC.World;

namespace TAC.Logic
{
	/// <summary>
	/// Represents the units controlled by a team
	/// Stores hostility and relations between other teams
	/// </summary>
	public class Team
	{
		// Team name
		public string Name { get; private set; }
		// Should be managed by AI controller
		public bool IsControlledByAI { get; internal set; }
		// Team members
		public List<Unit> Members;

		public Team(string TeamName, bool IsControlledByAI)
		{
			Name = TeamName;
			Members = new List<Unit>();
			this.IsControlledByAI = IsControlledByAI;
		}

		public void AddUnit(Unit newMember)
		{
			// Don't check faction yet. Let scene handle it
			//if(newMember.Faction)
			if (!Members.Contains(newMember)) Members.Add(newMember);
		}

		public bool HasUnit(Unit unit)
		{
			return Members.Contains(unit);
		}

		public bool AllUnitsDone()
		{
			foreach (Unit unit in Members)
				if (!unit.isDone) return false;
			return true;
		}

		// Helps with debugging I guess
		public override string ToString() => Name;
	}
}
