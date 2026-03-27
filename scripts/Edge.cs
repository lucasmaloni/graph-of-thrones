using Godot;
using System;

public partial class Edge
{
    public GreatHouse To { get; private set; }
    public double Intensity { get; set; } // Peso da aresta - provavelmente vai definir também a largura da aresta no grafo

    public Edge(GreatHouse To, double Intensity)
    {
        this.To = To;
        this.Intensity = Intensity;
    }
}