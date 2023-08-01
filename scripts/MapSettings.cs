/**
 * MapSettins.cs
 *
 * Speichert die Einstellungen fuer die aktuell generierte Karte
 */

public partial class MapSettings : Node
{
	public static float GrassLevel
	{
		get => _grassLevel;
		set
		{
			_grassLevel = value / 1000f;
		}
	}

	public static float HillLevel
	{
		get => _hillLevel;
		set
		{
			_hillLevel = value / 1000f;
		}
	}

	public static float RockLevel
	{
		get => _rockLevel;
		set
		{
			_rockLevel = value / 1000f;
		}
	}

	public static float SnowLevel
	{
		get => _snowLevel;
		set
		{
			_snowLevel = value / 1000f;
		}
	}

	private static float _grassLevel = 0.2f;
	private static float _hillLevel = 0.5f;
	private static float _rockLevel = 0.65f;
	private static float _snowLevel = 0.75f;

	public static Color COLOR_TERRAIN_WATER = new("#1E53FF");
	public static Color COLOR_TERRAIN_GRASS = new("#37B20A");
	public static Color COLOR_TERRAIN_HILL = new("#85CC24");
	public static Color COLOR_TERRAIN_ROCK = new("#707070");
	public static Color COLOR_TERRAIN_SNOW = new("#FFFFFF");

	public static int maxHeightValue = 255;
}