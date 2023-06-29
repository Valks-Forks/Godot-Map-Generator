using Godot;
using System;

/**
 * NoiseManager.cs
 *
 * Managed die verschiedenen Noises fuer
 *	Hoehe (elevation)
 *	Biome (biomes)
 *
 */
public partial class NoiseManager : Node {
	static FastNoiseLite defaultNoise = new FastNoiseLite();

	public FastNoiseLite debugNoise { get; set; } = new FastNoiseLite(); // TODO: aendern

	float[,] rawNoiseValues;

	// gibt die Karte, wleche generiert wurde, zurueck
	public ImageTexture getGeneratedMapImage(int width, int height) {
		GD.Print("-- generating image...");
		rawNoiseValues = new float[width, height];

		float minValue = 1f;
		float maxValue = -1f;

		// errechnet die Werte fuer jede einzelne Noise, lagert sie uebereinander
		// und normalisiert sie dann auf die Anzahl der Noises, findet gleichzeitig
		// minValue und maxValue zur skalierung heraus
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				rawNoiseValues[i, j] = debugNoise.GetNoise2D(i, j);
				if (rawNoiseValues[i, j] > 1f) {
					rawNoiseValues[i, j] = 1f;
					GD.Print("---- noise: value highered");
				} else if (rawNoiseValues[i, j] < -1f) {
					rawNoiseValues[i, j] = - 1f;
					GD.Print("---- noise: value lowered");
				}
				// Werte fuer die Skalierung
				if (rawNoiseValues[i, j] > maxValue) {
					maxValue = rawNoiseValues[i, j];
				} else if (rawNoiseValues[i, j] < minValue) {
					minValue = rawNoiseValues[i, j];
				}
			}
		}

		GD.Print("--- calculating noise values");
		float[,] scaledNoiseValues = rawNoiseValues;
		// skaliert die Werte auf den Bereich zwischen 0 und 1
		// Skalierungsfunktion:
		//         x - min                                  max - min
		// f(x) = ---------   ===>   f(min) = 0;  f(max) =  --------- = 1
		//        max - min                                 
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				scaledNoiseValues[i, j] = ((rawNoiseValues[i, j] - minValue) / (maxValue - minValue));
			}
		}

		GD.Print("--- setting pixels of image");
		Image image = Image.Create(width, height, false, Image.Format.Rgb8);
		// setzt die Pixel fuer das Image
		for (int i = 0; i < image.GetWidth(); i++) {
			for (int j = 0; j < image.GetHeight(); j++) {
				if (scaledNoiseValues[i, j] <= MapSettings.GrassLevel) {
					image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_WATER);
				} else if (scaledNoiseValues[i, j] <= MapSettings.HillLevel) {
					image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_GRASS);
				} else if (scaledNoiseValues[i, j] <= MapSettings.RockLevel) {
					image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_HILL);
				} else if (scaledNoiseValues[i, j] <= MapSettings.SnowLevel) {
					image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_ROCK);
				} else {
					image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_SNOW);
				}
				
				// else if (scaledNoiseValues[i, j] <= MapSettings.SNOW_LEVEL) {
				// 	image.SetPixel(i, j, MapSettings.COLOR_TERRAIN_SNOW);
				// }
				
			}
		}

		ImageTexture imageTexture = ImageTexture.CreateFromImage(image);
		return imageTexture;

	}

	// gibt eine standard ImageTexture zum Testen zurück
	public static ImageTexture getDefaultImageTexture(int width, int height) {
		int randomNumber = (int)GD.Randi();
		defaultNoise.Seed = randomNumber;
		return ImageTexture.CreateFromImage(defaultNoise.GetImage(width, height));
	}
}
