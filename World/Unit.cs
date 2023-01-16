﻿using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Nodes;
using TAC.Editor;
using TAC.Logic;

namespace TAC.World
{
	public enum UnitDirection
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
	};

	public enum UnitAnimation
	{
		Idle = 0,
		RunN
	}

	/// <summary>
	/// Gameplay represenation of a unit that moves and interacts with a scene.<br></br>
	/// Not for database representation, use something else.
	/// All values here are volatile and constantly changing, owned by the game scene
	/// </summary>

	public class Unit
	{
		public static readonly Vector3[] VectorDirections = {
				Vector3.Normalize(Vector3.UnitZ),
				Vector3.Normalize(Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ+Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitZ),
				Vector3.Normalize(-Vector3.UnitZ-Vector3.UnitX),
				Vector3.Normalize(-Vector3.UnitX),
				Vector3.Normalize(Vector3.UnitZ-Vector3.UnitX)
		};

		public UnitTemplate Template;

		public string Name;
		/// <summary>
		/// Index of team in scene team list
		/// TODO replace with Team reference
		/// </summary>
		public int TeamID;
		public int TimeUnits;
		public int Health;
		public List<Item> inventory;

		public Position position;
		public UnitDirection direction;

		// Chest height I guess
		public Vector3 equipOffset = new Vector3(0, 0.6f, 0);

		// General purpose counter
		public int animationPhase;
		public UnitAnimation animationState;

		public UnitAIModule UnitAI;
		public bool isDone;

		public Unit(UnitTemplate template, Position position, string name, UnitDirection direction = UnitDirection.North, List<Item> inventory = null)
		{
			this.Template = template;
			this.position = position;
			this.direction = direction;
			this.Name = name;
			this.animationPhase = 0;
			this.animationState = UnitAnimation.Idle;
			// We want our own copy
			this.TimeUnits = template.TimeUnits;
			this.Health = template.Health;
			this.inventory = inventory == null ? new List<Item>() : inventory;

			UnitAI = null;
		}

		/// <summary>
		/// Cannot fetch template by itself
		/// </summary>
		public Unit(JsonObject unitjson)
		{
			Name = (string)unitjson["Name"];
			Template = null; // MAKE SURE YOU FILL IT IN

			TeamID = (int)unitjson["TeamID"];
			TimeUnits = (int)unitjson["TimeUnits"];
			Health = (int)unitjson["Health"];

			position = new Position(unitjson["Position"].AsArray());
			direction = (UnitDirection)(int)unitjson["Direction"];
		}

		// TODO complete json representation
		public JsonObject GetJsonNode()
		{
			JsonObject node = new JsonObject();
			node["Name"] = Name;
			node["Template"] = Template.assetname;

			node["TeamID"] = TeamID;
			node["TimeUnits"] = TimeUnits;
			node["Health"] = Health;

			node["Position"] = position.GetJsonNode();
			node["Direction"] = (int)direction;

			return node;
		}

		public void Think(float deltaTime)
		{
			if (Template.Type != UnitTemplate.TemplateType.Skeletal
				|| Template.Animations.Length <= 0) return;

			ModelAnimation anim;
			if (animationState == UnitAnimation.RunN) {
				// runN
				anim = Template.Animations[1];
			} else {
				// idle
				anim = Template.Animations[0];
			}

			int framecount = anim.animation.frameCount;
			animationPhase = (animationPhase + 1) % framecount;
			Raylib_cs.Raylib.UpdateModelAnimation(Template.Model.model, anim.animation, animationPhase);
		}

		/// <summary>
		/// Return false if unit should be removed from game
		/// </summary>
		public bool IsAlive()
		{
			return Health > 0;
		}

		/// <summary>
		/// Return true if possible
		/// </summary>
		public bool AddToInventory(Item item)
		{
			if (inventory.Count <= 2) {
				inventory.Add(item);
				return true;
			}
			return false;
		}

		public Raylib_cs.BoundingBox GetUnitBoundingBox()
		{
			Raylib_cs.BoundingBox box = new(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 2, 0.5f));
			box.min = Raylib_cs.Raymath.Vector3Transform(box.min, Raylib_cs.Raymath.MatrixTranslate(position.x, position.y, position.z));
			box.max = Raylib_cs.Raymath.Vector3Transform(box.max, Raylib_cs.Raymath.MatrixTranslate(position.x, position.y, position.z));
			return box;
		}

		public void Reset()
		{
			if (UnitAI != null) UnitAI.Reset();
			TimeUnits = Template.TimeUnits;
			isDone = false;
		}
	}
}
