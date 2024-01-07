using Godot;
using System;

public partial class DebugHUD : Node
{

	[Export] private Label FPSLabel;
	[Export] private Label IsInAirLabel;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FPSLabel.Text = $"FPS: {Performance.GetMonitor(Performance.Monitor.TimeFps)}";
		IsInAirLabel.Text = $"Is In Air: {!Player.Instance.IsOnFloor()}";
	}
}
