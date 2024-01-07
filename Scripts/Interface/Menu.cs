using Godot;
using System;

public partial class Menu : Control
{
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    //handle button press

    //loads a game when pressed
    public void LoadBtnPressed() {
        GD.Print("InterfaceMenu: Load button pressed");
        
        //change scene to Scenes/Game.tscn
        GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
    }

    //connect to a multiplayer server
    public void ConnectBtnPressed() {
        GD.Print("InterfaceMenu: Connect button pressed");
    }

    //open settings menu
    public void SettingsBtnPressed() {
        GD.Print("InterfaceMenu: Settings button pressed");
    }

    //exit the game
    public void ExitBtnPressed() { 
        GD.Print("InterfaceMenu: Exit button pressed");
        //maybe do some saving before quitting
        GetTree().Quit();
    }
}
