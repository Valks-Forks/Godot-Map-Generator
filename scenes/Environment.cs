using Godot;
using System;

public partial class Environment : Node2D {

	////////////////
	// Referenzen //
	////////////////
	Sprite2D mapSprite;							// enthaelt die Karte als Textur
	Camera2D camera2D;							
	Timer timerGenerateMap;

	NoiseManager noiseManager = new NoiseManager();

	LineEdit leImageWidth, leImageHeight;
	LineEdit leSeed;
	LineEdit leGrassLevel, leHillLevel, leRockLevel, leSnowLevel;
	HSlider hslGrassLevel, hslHillLevel, hslRockLevel, hslSnowLevel;

	CheckBox cbRandomSeed;
	CheckBox cbGenerateMapAfterChange, cbGenerateWithoutDelay;

	ItemList itliNoiseType, itliFractalType;

	////////////////////
	// Hilfsvariablen //
	////////////////////
	// Mouse-Scrolling
	bool isMousePressed = false;
	Vector2 lastMousePosition = new Vector2();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

		camera2D = GetNode<Camera2D>("Camera2D");
		camera2D.MakeCurrent();

		timerGenerateMap = GetNode<Timer>("TimerGenerateMap");

		mapSprite = GetNode<Sprite2D>("MapSprite");

		leImageWidth = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxMapSize/HBoxWidthHeight/LeWidth");
		leImageHeight = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxMapSize/HBoxWidthHeight/LeHeight");

		leSeed = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxSeed/LeSeed");

		leGrassLevel = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxGrassLevel/LeGrassLevel");
		leHillLevel = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxHillLevel/LeHillLevel");
		leRockLevel = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxRockLevel/LeRockLevel");
		leSnowLevel = GetNode<LineEdit>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxSnowLevel/LeSnowLevel");

		hslGrassLevel = GetNode<HSlider>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxGrassLevel/HslGrassLevel");
		hslHillLevel = GetNode<HSlider>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxHillLevel/HslHillLevel");
		hslRockLevel = GetNode<HSlider>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxRockLevel/HslRockLevel");
		hslSnowLevel = GetNode<HSlider>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxSnowLevel/HslSnowLevel");

		cbRandomSeed = GetNode<CheckBox>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxSeed/CbRandomSeed");
		cbGenerateMapAfterChange = GetNode<CheckBox>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxCbs/CbGenerateMapAfterChange");
		cbGenerateWithoutDelay = GetNode<CheckBox>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Terrain/VBoxCbs/CbGenerateWithoutDelay");

		itliNoiseType = GetNode<ItemList>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Noise/VBoxNoiseType/ItLiNoiseType");
		itliNoiseType.Select(1); // default choice
		itliFractalType = GetNode<ItemList>("CanvasLayer/MenuContainer/MarginContainer/TabContainer/Noise/VBoxFractalType/ItLiFractalType");
		itliFractalType.Select(1); // default choice

		// Slider auf den aktuellen Wert setzen (falls dieser mal manuell geaendert wurde)
		leGrassLevel.Text = (MapSettings.GrassLevel * 1000).ToString();
		leHillLevel.Text = (MapSettings.HillLevel * 1000).ToString();
		leRockLevel.Text = (MapSettings.RockLevel * 1000).ToString();
		leSnowLevel.Text = (MapSettings.SnowLevel * 1000).ToString();
		hslGrassLevel.Value = MapSettings.GrassLevel * 1000;
		hslHillLevel.Value = MapSettings.HillLevel * 1000;
		hslRockLevel.Value = MapSettings.RockLevel * 1000;
		hslSnowLevel.Value = MapSettings.SnowLevel * 1000;
		generateMap();

	}


	public override void _Input(InputEvent @event) {
		
		// Mouse left click pressed
		if (@event is InputEventMouseButton eventMouseButtonPressed && 
				eventMouseButtonPressed.IsPressed() && 
				eventMouseButtonPressed.ButtonIndex == MouseButton.Left && 
				!Globals.isMouseOverUi) {
			isMousePressed = true;
			lastMousePosition = eventMouseButtonPressed.Position;
		}
		// Mouse left click released
		if (@event is InputEventMouseButton eventMouseButtonReleased && !@event.IsPressed() && eventMouseButtonReleased.ButtonIndex == MouseButton.Left) {
			isMousePressed = false;
		}
		// Mouse moved
		if (@event is InputEventMouseMotion eventMouseMotion) {
			if (isMousePressed) {
				Vector2 deltaMousePosition = lastMousePosition - eventMouseMotion.Position;
				lastMousePosition = eventMouseMotion.Position;
				// muss die Scrollgeschwindigkeit noch mit der aktuellen Zoom-stufe der camera2D skalieren
				camera2D.Position += deltaMousePosition * (1 / camera2D.Zoom.X);

			}
		}

		// Mouse wheel up (zoom)
		if (@event is InputEventMouseButton eventMouseWheelUp && eventMouseWheelUp.ButtonIndex == MouseButton.WheelUp) {
			Vector2 zoom = camera2D.Zoom;
			if (zoom.X <= 1f) {
				zoom *= 1.2f;
			}
			else {
				zoom *= 1.2f;
			}
			camera2D.Zoom = zoom;
			GD.Print(camera2D.Zoom);
		}
		// Mouse wheel down (zoom)
		if (@event is InputEventMouseButton eventMouseWheelDown && eventMouseWheelDown.ButtonIndex == MouseButton.WheelDown) {
			Vector2 zoom = camera2D.Zoom;
			if (zoom.X >= 1f) {
				zoom /= 1.2f;
			}
			else {
				zoom /= 1.2f;
			}
			camera2D.Zoom = zoom;
			GD.Print(camera2D.Zoom);
		}
	}


	private void generateMap() {
		int imageWidth, imageHeight;

		// Errorhandling
		try {
			imageWidth = leImageWidth.Text.ToInt();
			imageHeight = leImageHeight.Text.ToInt();
		} catch {
			GD.Print("-- ERROR: width or height of image is not a whole number");
			return;
		}
		
		////////////////////////
		// Parameter auslesen //
		////////////////////////
		//
		// Random Seed
		if (cbRandomSeed.ButtonPressed) {
			int newSeed = (int)GD.Randi();
			noiseManager.debugNoise.Seed = newSeed;
			leSeed.Text = newSeed.ToString();
			GD.Print("-- generating a map with new seed " + leSeed.Text);
		} else {
			try {
				noiseManager.debugNoise.Seed = leSeed.Text.ToInt();
				GD.Print("-- generating a map with seed " + leSeed.Text);
			} catch {
				GD.Print("-- ERROR: Seed in wrong format - use only numbers");
				return;
			}
		}
		// Hoehehlevel
		MapSettings.GrassLevel = leGrassLevel.Text.ToInt();
		MapSettings.HillLevel = leHillLevel.Text.ToInt();
		MapSettings.RockLevel = leRockLevel.Text.ToInt();
		MapSettings.SnowLevel = leSnowLevel.Text.ToInt();


		ImageTexture texture = noiseManager.getGeneratedMapImage(imageWidth, imageHeight);

		GetNode<Sprite2D>("MapSprite").Texture = texture;
	}

	// generiert die Karte
	private void _on_btn_generate_pressed()	{
		GD.Print("-- button pressed: generating image");
		generateMap();
	}



	//////////////////////////////////
	// Funktionen fuer Node-Signals //
	//////////////////////////////////

	// Grass HSlider wird geaendert
	private void _on_hsl_grass_level_value_changed(float value) {
		leGrassLevel.Text = value.ToString();
		// falls der value fuer grass hoeher ist als der fuer die anderen hoeheren Ebenen muessen diese angepasst werden
		if (value > leHillLevel.Text.ToInt()) {
			leHillLevel.Text = value.ToString();
			hslHillLevel.Value = value;
		}
		if (value > leRockLevel.Text.ToInt()) {
			leRockLevel.Text = value.ToString();
			hslRockLevel.Value = value;
		}
		if (value > leSnowLevel.Text.ToInt()) {
			leSnowLevel.Text = value.ToString();
			hslSnowLevel.Value = value;
		}
		timerGenerateMap.Start();
	}

	// Hill HSlider wird geaendert
	private void _on_hsl_hill_level_value_changed(float value) {
		leHillLevel.Text = value.ToString();
		// falls der value fuer hill hoeher ist als der fuer die anderen hoeheren Ebenen muessen diese angepasst werden
		if (value > leRockLevel.Text.ToInt()) {
			leRockLevel.Text = value.ToString();
			hslRockLevel.Value = value;
		}
		if (value > leSnowLevel.Text.ToInt()) {
			leSnowLevel.Text = value.ToString();
			hslSnowLevel.Value = value;
		}
		// falls der value fuer hill niedriger ist als der fuer die anderen niedrigeren Ebenen muss dieser angepasst werden
		if (value < leGrassLevel.Text.ToInt()) {
			leGrassLevel.Text = value.ToString();
			hslGrassLevel.Value = value;
		}
		timerGenerateMap.Start();
	}

	// Rock HSlider wird geaendert
	private void _on_hsl_rock_level_value_changed(float value) {
		leRockLevel.Text = value.ToString();
		// falls der value fuer rock hoeher ist als der fuer die anderen hoeheren Ebenen muss dieser angepasst werden
		if (value > leSnowLevel.Text.ToInt()) {
			leSnowLevel.Text = value.ToString();
			hslSnowLevel.Value = value;
		}
		// falls der value fuer rock niedriger ist als der fuer grass muss dieser angepasst werden
		if (value < leGrassLevel.Text.ToInt()) {
			leGrassLevel.Text = value.ToString();
			hslGrassLevel.Value = value;
		}
		if (value < leHillLevel.Text.ToInt()) {
			leHillLevel.Text = value.ToString();
			hslHillLevel.Value = value;
		}
		timerGenerateMap.Start();
	}

	// Snow HSlider wird geaendert
	private void _on_hsl_snow_level_value_changed(float value) {
		leSnowLevel.Text = value.ToString();
		// falls der value fuer snow hoeher ist als der fuer die anderen niedrigeren Ebenen muessen diese angepasst werden
		if (value < leGrassLevel.Text.ToInt()) {
			leGrassLevel.Text = value.ToString();
			hslGrassLevel.Value = value;
		}
		if (value < leHillLevel.Text.ToInt()) {
			leHillLevel.Text = value.ToString();
			hslHillLevel.Value = value;
		}
		if (value < leRockLevel.Text.ToInt()) {
			leRockLevel.Text = value.ToString();
			hslRockLevel.Value = value;
		}
		timerGenerateMap.Start();
	}

	// Grass LineEdit wird geaendert
	private void _on_le_grass_level_text_changed(string new_text) {
		if (new_text.Equals("")) {
			new_text = "0";
		}
		string lastChar = new_text.Substring(new_text.Length - 1);
		try {
			// falls das letzte Zeichen in einen int umgewandelt werden kann, ist es gueltig
			lastChar.ToInt();
			// limit fuer die Eingabe
			int newValue = new_text.ToInt(); 
			if (newValue > 1000) {
				newValue = 1000;	
			}
			hslGrassLevel.Value = newValue;
			leGrassLevel.Text = newValue.ToString();
			leGrassLevel.CaretColumn = leGrassLevel.Text.Length; // muss den caret zuruecksetzen
			timerGenerateMap.Start();
		} catch {
			// andernfalls wird es zurueckgesetzt
			GD.Print(new_text.Substr(0, new_text.Length - 1));
			leGrassLevel.Text = new_text.Substr(0, new_text.Length - 1);
			leGrassLevel.CaretColumn = leGrassLevel.Text.Length; // muss den caret zuruecksetzen
		}
	}

	// Hill LineEdit wird geaendert
	private void _on_le_hill_level_text_changed(string new_text) {
		if (new_text.Equals("")) {
			new_text = "0";
		}
		string lastChar = new_text.Substring(new_text.Length - 1);
		try {
			// falls das letzte Zeichen in einen int umgewandelt werden kann, ist es gueltig
			lastChar.ToInt();
			// limit fuer die Eingabe
			int newValue = new_text.ToInt();
			if (newValue > 1000) {
				newValue = 1000;	
			}
			hslHillLevel.Value = newValue;
			leHillLevel.Text = newValue.ToString();
			leHillLevel.CaretColumn = leHillLevel.Text.Length; // muss den caret zuruecksetzen
			timerGenerateMap.Start();
		} catch {
			// andernfalls wird es zurueckgesetzt
			GD.Print(new_text.Substr(0, new_text.Length - 1));
			leHillLevel.Text = new_text.Substr(0, new_text.Length - 1);
			leHillLevel.CaretColumn = leHillLevel.Text.Length; // muss den caret zuruecksetzen
		}
	}

	// Rock LineEdit wird geaendert
	private void _on_le_rock_level_text_changed(string new_text) {
		if (new_text.Equals("")) {
			new_text = "0";
		}
		string lastChar = new_text.Substring(new_text.Length - 1);
		try {
			// falls das letzte Zeichen in einen int umgewandelt werden kann, ist es gueltig
			lastChar.ToInt();
			// limit fuer die Eingabe
			int newValue = new_text.ToInt();
			if (newValue > 1000) {
				newValue = 1000;	
			}
			hslRockLevel.Value = newValue;
			leRockLevel.Text = newValue.ToString();
			leRockLevel.CaretColumn = leRockLevel.Text.Length; // muss den caret zuruecksetzen
			timerGenerateMap.Start();
		} catch {
			// andernfalls wird es zurueckgesetzt
			GD.Print(new_text.Substr(0, new_text.Length - 1));
			leRockLevel.Text = new_text.Substr(0, new_text.Length - 1);
			leRockLevel.CaretColumn = leRockLevel.Text.Length; // muss den caret zuruecksetzen
		}
	}

	// Snow LineEdit wird geaendert
	private void _on_le_snow_level_value_changed(string new_text) {
		if (new_text.Equals("")) {
			new_text = "0";
		}
		string lastChar = new_text.Substring(new_text.Length - 1);
		try {
			// falls das letzte Zeichen in einen int umgewandelt werden kann, ist es gueltig
			lastChar.ToInt();
			// limit fuer die Eingabe
			int newValue = new_text.ToInt();
			if (newValue > 1000) {
				newValue = 1000;	
			}
			hslSnowLevel.Value = newValue;
			leSnowLevel.Text = newValue.ToString();
			leSnowLevel.CaretColumn = leSnowLevel.Text.Length; // muss den caret zuruecksetzen
			timerGenerateMap.Start();
		} catch {
			// andernfalls wird es zurueckgesetzt
			GD.Print(new_text.Substr(0, new_text.Length - 1));
			leSnowLevel.Text = new_text.Substr(0, new_text.Length - 1);
			leSnowLevel.CaretColumn = leSnowLevel.Text.Length; // muss den caret zuruecksetzen
		}
	}

	// NoiseType wird geaendert
	private void _on_it_li_noise_type_item_selected(int index) {
		noiseManager.debugNoise.NoiseType = (FastNoiseLite.NoiseTypeEnum)index;
		timerGenerateMap.Start();
	}

	// FractalType wird geaendert
	private void _on_it_li_fractal_type_item_selected(int index) {
		noiseManager.debugNoise.FractalType = (FastNoiseLite.FractalTypeEnum)index;
		timerGenerateMap.Start();
	}

	private void _on_cb_generate_map_after_change_toggled(bool button_pressed) {
		GD.Print("toggling map generation after value change");
		if (!button_pressed) {
			cbGenerateWithoutDelay.ButtonPressed = false;
		}
	}

	private void _on_cb_generate_without_delay_toggled(bool button_pressed) {
		GD.Print("-- toggling instant generation");
		if (button_pressed) {
			cbGenerateMapAfterChange.ButtonPressed = true;
			timerGenerateMap.WaitTime = 0.01f;
		} else {
			timerGenerateMap.WaitTime = 0.5f;
		}
	}

	// generiert die neue Karte automatisch, falls die Option gewaehlt wurde
	private void _on_timer_generate_map_timeout() {
		if (cbGenerateMapAfterChange.ButtonPressed) {
			generateMap();
		}
	}

}
