using Godot;
using System;

public partial class Label : Godot.Label
{
    protected string fpsCounterText = "FPS: ";
    public override void _Process(double delta)
    {
        base._Process(delta);
        Text = fpsCounterText + string.Format("{0:F2}", Engine.GetFramesPerSecond());
    }
}
