using Godot;
using System;

public partial class Controller : Node2D
{
    [Export] Button RunReasonerButton { get; set; }
    [Export] GraphController GraphController { get; set; }

    public override void _Ready()
    {
        RunReasonerButton.Pressed += OnRunReasonerPressed;
    }

    public void OnRunReasonerPressed()
    {
        GraphController.InferGreatHousesConnections();
        GraphController.InferKingdomConnections();
    }
}
