using Godot;
using System;

public partial class Crosshair : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        //center crosshair
        this.Position = new Vector2(GetWindow().Size.X / 2, GetWindow().Size.Y / 2);
	}
}
