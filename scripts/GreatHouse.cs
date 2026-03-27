using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class GreatHouse : Sprite2D
{
    [Export] 
    public string HouseName { get; set; }
    public Vector2 targetDimensions { get; set; } = new Vector2(32, 32);

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(HouseName))
        {
            string path = $"res://assets/icons/{HouseName}.svg";
 
            if (ResourceLoader.Exists(path))
            {
                Texture = GD.Load<Texture2D>(path);
                Vector2 textureDimension = Texture.GetSize();
                
                Scale = targetDimensions / textureDimension;

                GD.Print($"escala final: {Scale}");
            }
            else
            {
                GD.PrintErr($"FALHA: O arquivo de icone {HouseName}.svg não existe");
            }
        }
    }

}
