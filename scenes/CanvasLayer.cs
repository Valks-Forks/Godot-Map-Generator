using Godot;
using System;

/**
 * CanvasLayer.cs
 *
 * Sub Node von Environment.cs. Zum Handling der UI-Hover
 */
public partial class CanvasLayer : Godot.CanvasLayer
{

	public void _on_ui_element_mouse_entered() {
		Globals.isMouseOverUi = true;
	}

	public void _on_ui_element_mouse_exited() {
		Globals.isMouseOverUi = false;
	}
}
