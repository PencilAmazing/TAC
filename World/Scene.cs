using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Nodes;
using TAC.Editor;
using TAC.Logic;
using TAC.Render;
using static System.Math;
using static TAC.World.Position;

namespace TAC.World
{
	public partial class Scene
	{
		public List<Team> Teams;
		public List<Unit> units;
		public List<ParticleEffect> particleEffects;

		/// <summary>
		/// Index of team in play
		/// </summary>
		public int CurrentTeamInPlay;

		private SceneTileSpace TileSpace;
		public Position Size { get => TileSpace.Size; }

		// Map tile types to texture names
		// Refreshed each map load
		// Used to translate between references during disk IO
		public List<Texture> TileTypeMap { get; private set; }
		public int AddTileToMap(Texture tile)
		{
			TileTypeMap.Add(tile);
			return TileTypeMap.Count - 1;
		}
		public List<Brush> BrushTypeMap { get; private set; }
		public int AddBrushToMap(Brush brush)
		{
			BrushTypeMap.Add(brush);
			return BrushTypeMap.Count - 1;
		}
		public List<Thing> ThingTypeMap { get; private set; }
		public int AddThingToMap(Thing thing)
		{
			ThingTypeMap.Add(thing);
			return ThingTypeMap.Count - 1;
		}

		// Runtime only data
		public ResourceCache cache;
		public Renderer renderer;
		public bool isEdit;
		public List<Position> viewCache;
		// TODO: replace with action stack?
		private Action currentAction;
		//

		public Scene(Renderer renderer, ResourceCache cache, bool isEdit)
		{
			this.renderer = renderer;
			this.cache = cache;
			this.isEdit = isEdit;

			currentAction = null;

			particleEffects = new List<ParticleEffect>();
			units = new List<Unit>();
			Teams = new List<Team>();

			TileTypeMap = new List<Texture>();
			BrushTypeMap = new List<Brush>();
			ThingTypeMap = new List<Thing>();
		}

		~Scene()
		{
			TileTypeMap.Clear();
			BrushTypeMap.Clear();
		}

		public void SetTileSpace(SceneTileSpace TileSpace)
		{
			this.TileSpace = TileSpace;
			// Just in case
			this.TileSpace.GenerateFloorMeshes(TileTypeMap, cache);
		}

		public void RegenerateTileSpaceFloor(int y)
		{
			if (TileSpace != null) TileSpace.UpdateFloorTexture(TileTypeMap, cache, y);
		}

		public void UpdateTileSpace() => TileSpace.UpdateFloorTextures(TileTypeMap, cache);

		public void EndTurn()
		{
			// No effect if unit is still moving
			if (GetCurrentAction() != null) return;

			Team currentTeam = Teams[CurrentTeamInPlay];
			for (int i = 0; i < currentTeam.Members.Count; i++) {
				// Reset each unit and unitAI in team
				currentTeam.Members[i].Reset();
			}

			// Switch to next team
			CurrentTeamInPlay += 1;
			CurrentTeamInPlay %= Teams.Count;
		}

		public virtual void Think(float deltaTime)
		{
			Team currentTeam = Teams[CurrentTeamInPlay];

			// If AI in play and no action in progress, get new action to perform
			if (currentTeam.IsControlledByAI && !CurrentActionInProgress()) {
				// Search for a unit we didn't process fully yet
				// All because selectedUnit is per player instead of per scene
				// TODO iterate on player controllers instead of teams directly
				foreach (Unit unit in currentTeam.Members) {
					if (unit.UnitAI != null && unit.UnitAI.StillThinking()) {
						SetCurrentAction(unit.UnitAI.Think());
						break;
					}
				}
			}

			if (currentAction != null) {
				if (currentAction.isDone)
					ClearCurrentAction();
				else
					currentAction.Think(deltaTime);
			}

			// Tick animations
			foreach (Unit unit in units) {
				unit.Think(deltaTime); // Here we go
			}

			if (currentTeam.IsControlledByAI && currentTeam.AllUnitsDone()) EndTurn();

			// Copy list
			foreach (ParticleEffect effect in particleEffects.ToArray()) {
				if (effect.phase < 0) particleEffects.Remove(effect);
			}
		}

		public virtual void Draw(Camera3D camera)
		{
			renderer.DrawSkybox(camera, cache);
			for (int i = 0; i < TileSpace.Height; i++) {
				renderer.DrawFloor(cache, GetFloorModel(i), TileTypeMap);
			}

			// Draw tile accessories
			for (int k = 0; k < TileSpace.Height; k++) {
				for (int i = 0; i < TileSpace.Length; i++) {
					for (int j = 0; j < TileSpace.Width; j++) {
						Tile tile = TileSpace[i, k, j];
						if (tile.North > 0)
							renderer.DrawWall(camera, new Vector3(i, k, j), false, tile.HasWall(Wall.FlipNorth), BrushTypeMap[tile.North - 1], cache);
						if (tile.West > 0)
							renderer.DrawWall(camera, new Vector3(i, k, j), true, tile.HasWall(Wall.FlipWest), BrushTypeMap[tile.West - 1], cache);
						if (tile.HasThing())
							renderer.DrawThing(ThingTypeMap[tile.thing - 1], new Vector3(i, k, j));
					}
				}
			}

			// Unit render loop
			foreach (Unit unit in units) renderer.DrawUnit(camera, unit, cache);

			// Particle render loop
			foreach (ParticleEffect effect in particleEffects) {
				effect.phase += 1;
				renderer.DrawEffect(camera, effect, cache);
			}

			renderer.DrawUnitDebug(camera, units, cache);
		}

		public virtual void DrawDebug3D(Camera3D camera)
		{
			//if (debugPath != null) renderer.DrawDebugPath(debugPath.ToArray());
			//Raylib.DrawSphere(Vector3.Zero, 0.1f, Color.PINK);

			// FIXME move this to virtual functions in each class
			ActionMoveUnit move = GetCurrentAction() as ActionMoveUnit;
			if (move != null) {
				renderer.DrawDebugPath(move.path.path.ToArray());
			}
			ActionSelectTarget select = GetCurrentAction() as ActionSelectTarget;
			if (select != null) {
				renderer.DrawDebugPath(select.line);
				if (select.impactData.hit)
					renderer.DrawDebugLine(select.unit.position.ToVector3() + select.unit.equipOffset, select.impactData.Point, Color.BEIGE);
				else
					renderer.DrawDebugLine(select.unit.position.ToVector3() + select.unit.equipOffset, select.target.ToVector3(), Color.BEIGE);
			}

			foreach (Unit unit in units)
				Raylib.DrawBoundingBox(unit.GetUnitBoundingBox(), Color.ORANGE);

			if (viewCache != null) {
				renderer.DrawDebugPath(viewCache.ToArray());
			}
		}

		public Raylib_cs.Model GetFloorModel(int level) => TileSpace.GetFloorModel(level);

		public Tile GetTile(Position pos) => TileSpace.GetTile(pos);
		public void SetTile(Tile tile, Position pos) => TileSpace.SetTile(tile, pos);

		/// <summary>
		/// Is position within size of scene?
		/// </summary>
		public bool IsTileWithinBounds(Position pos) => IsTileWithinBounds(pos, Size);
		public static bool IsTileWithinBounds(Position pos, Position bounds)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
			pos.x < bounds.x && pos.y < bounds.y && pos.z < bounds.z;
		}

		/// <summary>
		/// Is tile a null tile?
		/// </summary>
		public bool IsTileInvalid(Position pos) => TileSpace.GetTile(pos).IsTileInvalid();
		/// <summary>
		/// Does tile contain a unit?
		/// </summary>
		public bool IsTileOccupied(Position pos)
		{
			Tile tile = TileSpace.GetTile(pos);
			return tile.IsTileInvalid() || tile.HasUnit(); // Or tile has object
		}

		/// <summary>
		/// Does tile contain a unit or a thing?
		/// </summary>
		public bool IsTileImpassable(Position pos)
		{
			Tile tile = TileSpace.GetTile(pos);
			return tile.IsTileInvalid() || tile.IsTileImpassable();
		}

		/// <summary>
		/// Does tile block line of sight or line of fire?
		/// </summary>
		public bool IsTileBlocking(Position pos)
		{
			Tile tile = TileSpace.GetTile(pos);
			return tile.IsTileInvalid() || tile.IsTileBlocking();
		}

		public void ToggleWall(Position pos, Wall wall)
		{
			// Do your magic, rosyln
			Wall select = (Wall)TileSpace[pos].walls;
			select ^= wall;
			// Clear flip bits if wall bits are not set
			//select &= (Wall)((int)select << 2 | (int)select);
			// Extract parts of wall structure
			Wall visible = select & (Wall.North | Wall.West);
			Wall flips = select & (Wall.FlipNorth | Wall.FlipWest);
			// Keep flip flags that are necessary
			flips &= (Wall)((byte)visible << 2);
			// Collect parts
			select = flips | visible;
			TileSpace[pos].walls = (byte)select;
		}
		public void SetWall(Position pos, Wall wall) => TileSpace[pos].walls = (byte)wall;
		public void ClearWall(Position pos, Wall wall) => TileSpace[pos].walls &= (byte)~wall;

		/// <summary>
		/// Return index of texture in <see cref="TileTypeMap"/>. Adds to map if not found.
		/// </summary>
		public int GetTileTypeIndexOf(Texture texture)
		{
			int index = TileTypeMap.IndexOf(texture);
			if (index == -1) {
				TileTypeMap.Add(texture);
				index = TileTypeMap.Count - 1;
			}
			return index;
		}

		public int GetThingTypeIndexOf(Thing thing)
		{
			int index = ThingTypeMap.IndexOf(thing);
			return index != -1 ? index : AddThingToMap(thing);
		}

		public void ToggleThing(Position pos, Thing thing) => TileSpace[pos].thing = GetThingTypeIndexOf(thing);
		public void ToggleThing(Position pos, int thingIndex) => TileSpace[pos].thing = thingIndex;

		public void ToggleBrush(Position pos, Wall wall, Brush brush)
		{
			if (!Brush.IsBrushValid(brush)) return;
			int brushID = BrushTypeMap.IndexOf(brush) + 1;
			if (brushID == 0) {
				// brush not preloaded
				brushID = AddBrushToMap(brush);
			}
			ToggleBrush(pos, wall, brushID);
		}

		/// <summary>
		/// Calls ToggleWall aswell
		/// </summary>
		public void ToggleBrush(Position pos, Wall wall, int brushID)
		{
			if (wall.HasFlag(Wall.North)) {
				if (TileSpace[pos].North > 0)
					TileSpace[pos].North = 0;
				else
					TileSpace[pos].North = brushID;
				ToggleWall(pos, wall);
			} else if (wall.HasFlag(Wall.West)) {
				if (TileSpace[pos].West > 0)
					TileSpace[pos].West = 0;
				else
					TileSpace[pos].West = brushID;
				ToggleWall(pos, wall);
			}
		}

		public void ClearBrush(Position pos, Wall wall)
		{
			if (wall == Wall.North) {
				TileSpace[pos].North = 0;
				ClearWall(pos, Wall.North | Wall.FlipNorth);
			} else if (wall == Wall.West) {
				TileSpace[pos].West = 0;
				ClearWall(pos, Wall.West | Wall.FlipWest);
			}
		}

		// TODO move this to TileSpace.cs
		public Position[] GetSupercoverLine(Position origin, Position end) => GetSupercoverLine(origin.ToVector3(), end.ToVector3());

		// https://www.redblobgames.com/grids/line-drawing.html#supercover
		// https://github.com/cgyurgyik/fast-voxel-traversal-algorithm/blob/master/overview/FastVoxelTraversalOverview.md
		// https://gitlab.com/athilenius/fast-voxel-traversal-rs/-/blob/main/src/raycast_3d.rs
		// Supercover line does include initial tile in result because line technically passes through it
		public Position[] GetSupercoverLine(Vector3 origin, Vector3 end)
		{
			Vector3 d = Vector3.Normalize(end - origin);

			int stepX = Sign(d.X);
			int stepY = Sign(d.Y);
			int stepZ = Sign(d.Z);

			// Starting voxel
			Position i = new Position(origin);
			List<Position> points = new List<Position>() { new Position(origin) };

			if (Vector3.DistanceSquared(end, origin) < float.Epsilon) return points.ToArray();

			float tDeltaX = Abs(d.X) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.X);
			float tDeltaY = Abs(d.Y) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.Y);
			float tDeltaZ = Abs(d.Z) < float.Epsilon ? float.PositiveInfinity : Abs(1 / d.Z);

			// FIXME Cell is not a cube, Y=2.0f you should really put it in a constant somewhere
			// Offset by 0.5f to find fractional part, since out tiles are centered at 0,0
			// Other algorithms have tiles centered at 0.5, 0.5, so we try to cancel it out
			// Dont try to remove this \/, this is cell size and is 1.0f for now
			float distX = stepX > 0 ? (1.0f - 0.5f - origin.X + i.x) : (origin.X - i.x + 0.5f);
			float distY = stepY > 0 ? (1.0f - 0.5f - origin.Y + i.y) : (origin.Y - i.y + 0.5f);
			float distZ = stepZ > 0 ? (1.0f - 0.5f - origin.Z + i.z) : (origin.Z - i.z + 0.5f);

			float tMaxX = tDeltaX < float.PositiveInfinity ? distX * tDeltaX : float.PositiveInfinity;
			float tMaxY = tDeltaY < float.PositiveInfinity ? distY * tDeltaY : float.PositiveInfinity;
			float tMaxZ = tDeltaZ < float.PositiveInfinity ? distZ * tDeltaZ : float.PositiveInfinity;

			while (IsTileWithinBounds(i)) {
				if (tMaxX < tMaxY) {
					if (tMaxX < tMaxZ) {
						i.x += stepX;
						tMaxX += tDeltaX;
					} else {
						i.z += stepZ;
						tMaxZ += tDeltaZ;
					}
				} else {
					if (tMaxY < tMaxZ) {
						i.y += stepY;
						tMaxY += tDeltaY;
					} else {
						i.z += stepZ;
						tMaxZ += tDeltaZ;
					}
				}
				points.Add(i);
			}
			return points.ToArray();
		}

		public List<Position> GetUnitFOV(Unit unit)
		{
			if (unit == null) return new();
			return GetUnitVisibleTiles(unit.position, unit.direction);
		}

		/// <summary>
		/// True if adjacent direction is blocked
		/// </summary>
		/// <param name="dir">Direction to walk towards</param>
		public bool TestDirection(Position pos, UnitDirection dir)
		{
			// I really hope the compiler fixes this
			int x = pos.x;
			int z = pos.z;

			// Blocking cardinal direction movement
			bool north = TileSpace.GetTile(pos).HasWall(Wall.North) || IsTileImpassable(pos - PositiveZ);
			bool west = TileSpace.GetTile(pos).HasWall(Wall.West) || IsTileImpassable(pos - PositiveX);
			bool south = TileSpace.GetTile(pos + PositiveZ).HasWall(Wall.North) || IsTileImpassable(pos + PositiveZ);
			bool east = TileSpace.GetTile(pos + PositiveX).HasWall(Wall.West) || IsTileImpassable(pos + PositiveX);

			bool northeast = TileSpace.GetTile(pos + PositiveX - PositiveZ).HasWall(Wall.West) ||
							 TileSpace.GetTile(pos + PositiveX).HasWall(Wall.North) ||
							 IsTileImpassable(pos + PositiveX - PositiveZ);
			bool northwest = TileSpace.GetTile(pos - PositiveZ).HasWall(Wall.West) ||
							 TileSpace.GetTile(pos - PositiveX).HasWall(Wall.North) ||
							 IsTileImpassable(pos - PositiveX - PositiveZ);
			bool southeast = TileSpace.GetTile(pos + PositiveX + PositiveZ).HasWall(Wall.North) ||
							 TileSpace.GetTile(pos + PositiveX + PositiveZ).HasWall(Wall.West) ||
							 IsTileImpassable(pos + PositiveX + PositiveZ);
			bool southwest = TileSpace.GetTile(pos - PositiveX + PositiveZ).HasWall(Wall.North) ||
							 TileSpace.GetTile(pos + PositiveZ).HasWall(Wall.West) ||
							 IsTileImpassable(pos + PositiveZ - PositiveX);

			if (dir == UnitDirection.North)
				return north;
			if (dir == UnitDirection.West)
				return west;
			if (dir == UnitDirection.South)
				return south;
			if (dir == UnitDirection.East)
				return east;

			if (dir == UnitDirection.NorthEast) {
				return north || east || northeast;
			} else if (dir == UnitDirection.NorthWest) {
				return north || west || northwest;
			} else if (dir == UnitDirection.SouthEast) {
				return south || east || southeast;
			} else if (dir == UnitDirection.SouthWest) {
				return south || west || southwest;
			} else return false;
		}

		public Action GetCurrentAction() => currentAction;
		public void SetCurrentAction(Action action) => currentAction = action;
		public void ClearCurrentAction() => currentAction = currentAction.NextAction();
		public bool CurrentActionInProgress() => currentAction != null;

		/// <summary>
		/// Add unit to scene.
		/// </summary>
		public bool AddUnit(Unit unit, int factionID, UnitAIModule aiModule = null)
		{
			// Check arguments validity
			if (unit == null || IsTileOccupied(unit.position) ||
				factionID < 0 || factionID > Teams.Count) return false;

			// Bind to team
			Teams[factionID].AddUnit(unit);
			unit.TeamID = factionID;

			// Add unit to game scene
			units.Add(unit);
			Tile tile = TileSpace.GetTile(unit.position);
			tile.unit = unit;
			TileSpace.SetTile(tile, unit.position);

			// Set unit AI
			unit.UnitAI = aiModule;

			return true;
		}

		public bool AddUnit(Unit unit, Team team)
		{
			int factionID = Teams.IndexOf(team);
			return AddUnit(unit, factionID);
		}

		/// <summary>
		/// UNIMPLEMENTED. Could be an alternative to storing faction ID in Unit
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public Team GetUnitTeam(Unit unit)
		{
			throw new System.NotImplementedException();
			//foreach(Team team in teams) {
			//	if (team.HasUnit(unit)) return team;
			//}
			//return null;
		}

		public void AddTeam(Team team)
		{
			if (!Teams.Contains(team)) Teams.Add(team);
		}

		public Team GetCurrentTeamInPlay() => Teams[CurrentTeamInPlay];
		public bool IsTeamInPlay(Team team) => team == GetCurrentTeamInPlay();

		public void PushActionMoveUnit(Unit unit, Position goal)
		{
			if (unit == null || currentAction != null) return;
			Pathfinding path = new Pathfinding(this);
			if (path.FindPathForUnit(unit, goal)) {
				SetCurrentAction(new ActionMoveUnit(this, path));
			}
		}

		public void PushActionSelectTarget(Unit unit, Item item, Position target)
		{
			// Don't push action if unit is null or there's 
			// already an action in place
			if (unit == null || currentAction != null) return;
			if (IsTileWithinBounds(target)) {
				SetCurrentAction(new ActionSelectTarget(this, unit, item, target));
			}
		}

		public void PushActionTurnUnit(Unit unit, Position target)
		{
			if (unit != null && currentAction == null) {
				SetCurrentAction(new ActionTurnUnit(this, unit, Pathfinding.GetDirection(unit.position, target)));
			}
		}

		public Unit GetUnit(Position pos) => TileSpace.GetTile(pos).unit;

		/// <summary>
		/// Moves a unit from it's old position to <paramref name="newPos"/>
		/// </summary>
		public void MoveUnitToTile(Unit unit, Position newPos)
		{
			// Remove reference to unit in old tile
			TileSpace[unit.position.x, unit.position.y, unit.position.z].unit = null;
			// Update position
			unit.position = newPos;
			// Set reference in new tile
			TileSpace[newPos.x, newPos.y, newPos.z].unit = unit;
		}

		// health only for now
		public void AffectUnit(Unit unit, int effect)
		{
			unit.Health += effect;
		}

		public void AddParticleEffect(ParticleEffect effect)
		{
			if (effect != null) particleEffects.Add(effect);
		}

		public void RemoveParticleEffect(ParticleEffect effect)
		{
			if (effect != null) particleEffects.Remove(effect);
		}

		public JsonNode GetJsonNode()
		{
			JsonObject node = new JsonObject();
			node["TileTypeMap"] = new JsonArray();
			foreach (Texture tile in TileTypeMap) {
				((JsonArray)node["TileTypeMap"]).Add(tile.assetname);
			}
			node["BrushTypeMap"] = new JsonArray();
			foreach (Brush brush in BrushTypeMap) {
				((JsonArray)node["BrushTypeMap"]).Add(brush.assetname);
			}
			node["ThingTypeMap"] = new JsonArray();
			foreach (Thing thing in ThingTypeMap) {
				((JsonArray)node["ThingTypeMap"]).Add(thing.assetname);
			}

			node["Teams"] = new JsonArray();
			foreach (Team team in Teams) {
				node["Teams"].AsArray().Add(team.GetJsonNode());
			}

			node["Units"] = new JsonArray();
			foreach (Unit unit in units) {
				node["Units"].AsArray().Add(unit.GetJsonNode());
			}

			node["TileSpace"] = TileSpace.GetJsonNode();

			return node;
		}
		public bool FillFromJson(JsonObject sceneNode)
		{
			JsonArray tilearray = sceneNode["TileTypeMap"].AsArray();
			// Rebuild tile texture map
			foreach (string assetname in tilearray) {
				Texture tex = cache.GetTexture(assetname);
				TileTypeMap.Add(tex);
			}
			JsonArray brusharray = sceneNode["BrushTypeMap"].AsArray();
			foreach (string assetname in brusharray) {
				Brush brush = cache.GetBrush(assetname);
				BrushTypeMap.Add(brush);
			}
			JsonArray thingarray = sceneNode["ThingTypeMap"].AsArray();
			foreach (string assetname in thingarray) {
				Thing thing = cache.GetThing(assetname);
				ThingTypeMap.Add(thing);
			}

			JsonArray teamarray = sceneNode["Teams"].AsArray();
			foreach (JsonObject teamnode in teamarray) {
				//Team team = cache.GetTeam((string)teamnode["assetname"]);
				Team team = new Team(teamnode);
				Teams.Add(team);
			}

			JsonObject tilespace = sceneNode["TileSpace"].AsObject();
			SetTileSpace(new SceneTileSpace(tilespace));

			JsonArray unitarray = sceneNode["Units"].AsArray();
			foreach (JsonObject unitjson in unitarray) {
				UnitTemplate template = cache.GetUnitTemplate((string)unitjson["Template"]);
				Unit unit = new Unit(unitjson, template, new List<Item>());

				AddUnit(unit, unit.TeamID);
				unit.UnitAI = new UnitAIModule(this, unit);
			}

			return true;
		}
	}
}
