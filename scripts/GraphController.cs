using Godot;
using System;
using System.Collections.Generic;

public partial class GraphController : Node2D
{
	protected Dictionary<GreatHouse, List<Edge>> Graph { get; private set; } = new Dictionary<GreatHouse, List<Edge>>();

	public override void _Ready()
	{
		SetupInitialHouses();
	}

	private void AddConnection(GreatHouse from, GreatHouse to, double intensity)
	{
		/*
			Considerar melhorar essa adição de conexões:
			1. Ao invés de usar uma lista, usar um Dicionario<GreatHouse, Edge>
			2. Validar se ambos os vértices existem e se as conexões também existem
		*/

		if (!Graph.ContainsKey(from) || !Graph.ContainsKey(to)) return;
		
		Graph[from].Add(new Edge(to, intensity));
		Graph[to].Add(new Edge(from, intensity));
	}

	private void SetupInitialHouses()
	{
		GreatHouse stark = new GreatHouse("Stark", "to be defined");
		GreatHouse lanninster = new GreatHouse("Lanninster", "to be defined");
		GreatHouse baratheonRenly = new GreatHouse("Baratheon (Renly)", "to be defined");
		GreatHouse baratheonStanis = new GreatHouse("Baratheon (Stanis)", "to be defined");
		GreatHouse baratheonRobert = new GreatHouse("Baratheon (Stanis)", "to be defined");
		GreatHouse frey = new GreatHouse("Frey", "to be defined");
		GreatHouse bolton = new GreatHouse("Bolton", "to be defined");
		GreatHouse tully = new GreatHouse("Tully", "to be defined");
		GreatHouse karstark = new GreatHouse("Karstark", "to be defined");
		GreatHouse tyrell = new GreatHouse("Tyrell", "to be defined");

		Graph.Add(stark, new List<Edge>());
		Graph.Add(lanninster, new List<Edge>());
		Graph.Add(baratheonRenly, new List<Edge>());
		Graph.Add(baratheonStanis, new List<Edge>());
		Graph.Add(baratheonRobert, new List<Edge>());
		Graph.Add(frey, new List<Edge>());
		Graph.Add(bolton, new List<Edge>());
		Graph.Add(tully, new List<Edge>());
		Graph.Add(karstark, new List<Edge>());
		Graph.Add(tyrell, new List<Edge>());
	}

}
