using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class GreatHouse : RigidBody2D
{
	[Export] 
	public string HouseName { get; set; }
	public Sprite2D SigilSprite { get; private set; }
	public Vector2 targetDimensions { get; set; } = new Vector2(128, 141);
	// As dimensões do icone são 600x660 - de forma que as dimensões que queremos devem ser (x, 1.1x)
	// Tem forma melhor de fazer isso ao invés de só declarar cada uma, mas agora não vou priorizar essa otimização mínima

	public override void _Ready()
	{
		SigilSprite = GetNode<Sprite2D>("Sigil");
		GravityScale = 0; // Desativa a gravidade para que as casas não caiam
		
		if (!string.IsNullOrEmpty(HouseName))
		{
			string path = $"res://assets/icons/{HouseName}.svg";
 
			if (ResourceLoader.Exists(path))
			{
				SigilSprite.Texture = GD.Load<Texture2D>(path);
				Vector2 textureDimension = SigilSprite.Texture.GetSize();
				
				SigilSprite.Scale = targetDimensions / textureDimension;

			}
			else
			{
				GD.PrintErr($"FALHA: O arquivo de icone {HouseName}.svg não existe");
			}
		}
	}

}
