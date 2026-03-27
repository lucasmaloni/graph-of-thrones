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
		GreatHouse stark = new GreatHouse("Stark");
		GreatHouse lanninster = new GreatHouse("Lanninster");
		GreatHouse baratheonRenly = new GreatHouse("Baratheon (Renly)");
		GreatHouse baratheonStanis = new GreatHouse("Baratheon (Stanis)");
		GreatHouse baratheonRobert = new GreatHouse("Baratheon (Stanis)");
		GreatHouse frey = new GreatHouse("Frey");
		GreatHouse bolton = new GreatHouse("Bolton");
		GreatHouse tully = new GreatHouse("Tully");
		GreatHouse karstark = new GreatHouse("Karstark");
		GreatHouse tyrell = new GreatHouse("Tyrell");
		GreatHouse mormont = new GreatHouse("Mormont");
		GreatHouse glover = new GreatHouse("Glover");
		GreatHouse umber = new GreatHouse("Umber");
		GreatHouse ryswell = new GreatHouse("Ryswell");
		GreatHouse arryn = new GreatHouse("Arryn");
		GreatHouse greyjoy = new GreatHouse("Greyjoy");

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
		Graph.Add(mormont, new List<Edge>());
		Graph.Add(glover, new List<Edge>());
		Graph.Add(umber, new List<Edge>());
		Graph.Add(ryswell, new List<Edge>());
		Graph.Add(arryn, new List<Edge>());
		Graph.Add(greyjoy, new List<Edge>());
	}

}
