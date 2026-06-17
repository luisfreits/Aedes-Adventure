using Godot;
using System;

public partial class Creditos : Control
{
	private void OnBackBtnPressed()
    {
        GetTree().ChangeSceneToFile("res://scene/title_screen.tscn");
    }
}
