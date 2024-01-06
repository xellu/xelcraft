using Godot;
using System;

public partial class FpsCounter : Label
{
    public static int fps = 0;
    public static long fpsReset = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        fps++;
        if (DateTimeOffset.Now.ToUnixTimeSeconds() - fpsReset > 1) {
            this.Text = "FPS: " + fps.ToString();
            fps = 0;
            fpsReset = DateTimeOffset.Now.ToUnixTimeSeconds() + 1;
        }
	}
}
