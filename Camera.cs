using Godot;
using System;

public partial class Camera : Camera3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vector3 rot = new Vector3(player.pitch, 0, 0);
        this.Rotation = rot;
    }
}
