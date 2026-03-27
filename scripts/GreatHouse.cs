using Godot;
using System;
using System.Collections.Generic;

public partial class GreatHouse : Sprite2D
{

    public GreatHouse (string name)
    {
        Name = name;
        Texture = GD.Load<Texture2D>("res://assets/icons/"+name+".svg");
    }

}
