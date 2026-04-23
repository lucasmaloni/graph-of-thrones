using Godot;
using System;

public partial class NavigationCamera : Camera2D
{
    private float _targetZoom = 1.0f;
    private const float ZoomSpeed = 0.1f;
    private const float MinZoom = 0.2f;
    private const float MaxZoom = 2.0f;

    public override void _Input(InputEvent @event)
    {
        // Zoom com o Scroll do mouse
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                _targetZoom = Mathf.Clamp(_targetZoom + ZoomSpeed, MinZoom, MaxZoom);
            if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                _targetZoom = Mathf.Clamp(_targetZoom - ZoomSpeed, MinZoom, MaxZoom);
        }

        // Panning (Arrastar) com botão direito
        if (@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            Position -= mouseMotion.Relative / Zoom;
        }
    }

    public override void _Process(double delta)
    {
        // Interpolação suave para o zoom (Linear Interpolation)
        Zoom = Zoom.Lerp(new Vector2(_targetZoom, _targetZoom), (float)delta * 10f);
    }
}
