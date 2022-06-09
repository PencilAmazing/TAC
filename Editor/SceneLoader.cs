using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAC.World;

namespace TAC.Editor
{
	class SceneLoader
	{
		enum LoaderState
		{
			Brush,
			Units,
			Floor
		}

		LoaderState state;
		Scene scene;

		public SceneLoader(Scene scene)
		{
			this.scene = scene;
		}

		void SwitchState(string line)
		{
			if (line == "[Brush]") {
				state = LoaderState.Brush;
			} else if (line == "[Units]") {
				state = LoaderState.Units;
			} else if (line == "[Floor]") {
				state = LoaderState.Floor;
			}
		}

		void LoadBrush(string line)
		{
			int[] faces = new int[6];
			string[] chars = line.Split(',');
			for (int i = 0; i < 6; i++) {
				faces[i] = Convert.ToInt32(chars[i]);
			}
			scene.cache.CreateBrush(new Brush(faces));
		}

		void LoadUnit(string line)
		{
			string[] data = line.Split(',');

			int type = Convert.ToInt32(data[0]);
			string name = data[1];
			int x = Convert.ToInt32(data[2]);
			int y = Convert.ToInt32(data[3]);
			int z = Convert.ToInt32(data[4]);
			Position pos = new(x, y, z);
			int direction = Convert.ToInt32(data[5]);
			Unit unit = new Unit(type, pos, name, (UnitDirection)direction);
			scene.units.Add(unit);
			scene.floor.SetTileUnit(pos, unit);
		}

		void LoadFloor(string line)
		{
			string[] chars = line.Split(',');
			int length = Convert.ToInt32(chars[0]);
			int width = Convert.ToInt32(chars[1]);

			for (int i = 0+2; i < chars.Count(); i += 4) {
				int type = Convert.ToInt32(chars[i]);
				int north = Convert.ToInt32(chars[i+1]);
				int west = Convert.ToInt32(chars[i+2]);
				int thing = Convert.ToInt32(chars[i+3]);
				Tile tile = new Tile(type, north, west, thing);
			}
		}

		public void ProcessLine(string line)
		{
			if (line[0] == '[') {
				SwitchState(line);
				return;
			}

			switch (state) {
				case LoaderState.Brush:
					LoadBrush(line);
					break;
				case LoaderState.Units:
					LoadUnit(line);
					break;
				case LoaderState.Floor:
					LoadFloor(line);
					break;
				default:
					break;
			}
		}
	}
}
