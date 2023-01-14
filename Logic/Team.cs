using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
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
			this.IsControlledByAI = IsControlledByAI;
			Members = new List<Unit>();
		}

		public Team(JsonObject teamjson)
			: this((string)teamjson["Name"], (bool)teamjson["IsControlledByAI"])
		{
			//JsonArray membersarray = teamjson["Members"].AsArray();
			//Members = new List<Unit>(membersarray.Count);
			//foreach (JsonObject member in membersarray) {
			//	Members.Add(new Unit(member));
			//}
		}

		public JsonNode GetJsonNode()
		{
			JsonObject node = new JsonObject();
			node["Name"] = Name;
			node["IsControlledByAI"] = IsControlledByAI;
			//node["Members"] = new JsonArray();
			//foreach (Unit unit in Members) {
				//node["Members"].AsArray().Add(unit.GetJsonNode());
			//}

			return node;
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
