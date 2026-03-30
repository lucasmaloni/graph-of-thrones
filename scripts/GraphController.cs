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
		// Aqui estamos pegando as referências de nós da cena, nesse caso, 
		// cada nó desse tá como uma GreatHouse, por isso fazemose esse Get<GreatHouse>

		GreatHouse stark = GetNode<GreatHouse>("Stark");
		GreatHouse lanninster = GetNode<GreatHouse>("Lanninster");
		GreatHouse baratheonRenly = GetNode<GreatHouse>("Baratheon (Renly)");
		GreatHouse baratheonStanis = GetNode<GreatHouse>("Baratheon (Stanis)");
		GreatHouse baratheonRobert = GetNode<GreatHouse>("Baratheon (Robert)");
		GreatHouse frey = GetNode<GreatHouse>("Frey");
		GreatHouse bolton = GetNode<GreatHouse>("Bolton");
		GreatHouse tully = GetNode<GreatHouse>("Tully");
		GreatHouse karstark = GetNode<GreatHouse>("Karstark");
		GreatHouse tyrell = GetNode<GreatHouse>("Tyrell");
		GreatHouse mormont = GetNode<GreatHouse>("Mormont");
		GreatHouse glover = GetNode<GreatHouse>("Glover");
		GreatHouse umber = GetNode<GreatHouse>("Umber");
		GreatHouse ryswell = GetNode<GreatHouse>("Ryswell");
		GreatHouse arryn = GetNode<GreatHouse>("Arryn");
		GreatHouse greyjoy = GetNode<GreatHouse>("Greyjoy");

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

		GD.Print("Sucesso em adicionar nós ao grafo, segue o jogo");
	}

	private void SetupInitialRelations()
	{
		
	}
}
