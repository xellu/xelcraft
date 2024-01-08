using Godot;
using System;

public partial class Inventory : Control
{
    public bool IsOpen = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Visible != IsOpen) {Visible = IsOpen;}
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("container_toggle")) {
            IsOpen = !IsOpen;
            InterfaceManager.instance.SetContainer(IsOpen);
        }
    }
}
