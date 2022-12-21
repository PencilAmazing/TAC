namespace TAC.Editor
{
	/// <summary>
	/// Describes default values for a Unit
	/// </summary>
	public class UnitTemplate
	{
		public int Health;
		public int TimeUnits;

		public Texture Texture;

		public UnitTemplate(int health, int timeUnits, Texture texture)
		{
			Health = health;
			TimeUnits = timeUnits;
			Texture = texture;
		}
	}
}
