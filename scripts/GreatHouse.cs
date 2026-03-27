using Godot;
using System;
using System.Collections.Generic;

public partial class GreatHouse : Sprite2D
{
    public List<Edge> Connections { get; private set;} = new List<Edge>();
    // Caso seja possível (tempo), implementar um tamanho

    public GreatHouse (string name, string iconPath)
    {
        Name = name;
        Texture = GD.Load<Texture2D>(iconPath);
    }

    protected void AddConection(GreatHouse toHouse, double intensity)
    {
        Connections.Add(new Edge(toHouse, intensity));
        /* 
        toHouse.Connections.Add(new Edge(this, intensity)); Analisar uma forma de implementar a adição Casa X <-> Casa Y
        */
    }

}
