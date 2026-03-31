using Godot;
using GCollections = Godot.Collections;
using System.Collections.Generic;
using System;

public partial class GraphController : Node2D
{
	protected Dictionary<GreatHouse, Dictionary<GreatHouse, Edge>> Graph { get; private set; } = new Dictionary<GreatHouse, Dictionary<GreatHouse, Edge>>();
	protected Dictionary<string, GreatHouse> HouseLookup { get; private set; } = new Dictionary<string, GreatHouse>();
	protected DataManager dataManager = new DataManager();
	protected HashSet<Tuple<GreatHouse, GreatHouse>> drawnConnections = new HashSet<Tuple<GreatHouse, GreatHouse>>();

	public override void _Ready()
	{
		SetupInitialHouses();
		SetupHousesLookup();
		SetupInitialConnections();
	}

    public override void _Process(double delta)
    {
		// Esse método roda todo frame, é o comportamento do GraphController durante todo o programa
        base._Process(delta);

		QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();

		drawnConnections.Clear();
		foreach (GreatHouse from in Graph.Keys)
		{
			foreach (var to in Graph[from])
			{
				Tuple<GreatHouse, GreatHouse> connectionTuple = new Tuple<GreatHouse, GreatHouse>(from, to.Key);
				
				// Retorna caso ja tenhamos desenhado a relação casa A casa B, para evitar desenhar casa B com casa A
				if (drawnConnections.Contains(connectionTuple)) continue; 
				
				DrawLine(from.GlobalPosition, to.Key.GlobalPosition, new Color(1, (float)0.84313726, 0, 1), (float)to.Value.Intensity * 5);
				drawnConnections.Add(connectionTuple);
			}
		}
    }

	private void AddConnection(GreatHouse from, GreatHouse to, double intensity)
	{
		/*
			Considerar melhorar essa adição de conexões:
			1. Ao invés de usar uma lista, usar um Dicionario<GreatHouse, Edge>
			2. Validar se ambos os vértices existem e se as conexões também existem
		*/

		if (!Graph.ContainsKey(from) || !Graph.ContainsKey(to)) return;
		
		Graph[from].Add(to, new Edge(intensity));
		Graph[to].Add(from, new Edge(intensity));
	}

	private void SetupInitialHouses()
	{
		// Aqui estamos pegando as referências de nós da cena, nesse caso, 
		// cada nó desse tá como uma GreatHouse, por isso fazemose esse Get<GreatHouse>

		GreatHouse stark = GetNode<GreatHouse>("Stark");
		GreatHouse lannister = GetNode<GreatHouse>("Lannister");
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

		Graph.Add(stark, new Dictionary<GreatHouse, Edge>());
		Graph.Add(lannister, new Dictionary<GreatHouse, Edge>());
		Graph.Add(baratheonRenly, new Dictionary<GreatHouse, Edge>());
		Graph.Add(baratheonStanis, new Dictionary<GreatHouse, Edge>());
		Graph.Add(baratheonRobert, new Dictionary<GreatHouse, Edge>());
		Graph.Add(frey, new Dictionary<GreatHouse, Edge>());
		Graph.Add(bolton, new Dictionary<GreatHouse, Edge>());
		Graph.Add(tully, new Dictionary<GreatHouse, Edge>());
		Graph.Add(karstark, new Dictionary<GreatHouse, Edge>());
		Graph.Add(tyrell, new Dictionary<GreatHouse, Edge>());
		Graph.Add(mormont, new Dictionary<GreatHouse, Edge>());
		Graph.Add(glover, new Dictionary<GreatHouse, Edge>());
		Graph.Add(umber, new Dictionary<GreatHouse, Edge>());
		Graph.Add(ryswell, new Dictionary<GreatHouse, Edge>());
		Graph.Add(arryn, new Dictionary<GreatHouse, Edge>());
		Graph.Add(greyjoy, new Dictionary<GreatHouse, Edge>());

		GD.Print("Sucesso em adicionar nós ao grafo, segue o jogo");
	}


	private void SetupHousesLookup()
	{
		// Populamos o dicionário de lookup para termos a referência de cada casa a partir do nome.
		// O objetivo é ter a referência do nó a partir do nome, necessário para criação de conexões em SetupInitialRelations() e outras mudanças futuras
		foreach (var house in Graph.Keys)
		{
			HouseLookup[house.HouseName] = house;
		}
	}

	private void SetupInitialConnections()
	{
		GCollections.Array<Variant> InitialConections = dataManager.GetDataFromJson("InitialConnections");

		foreach (Variant Connection in InitialConections)
		{
			GCollections.Dictionary ConnectionAsDict = (GCollections.Dictionary)Connection;

			string houseFrom = (string)ConnectionAsDict["from"];
			string houseTo = (string)ConnectionAsDict["to"];
			double intensity = (double)ConnectionAsDict["intensity"];

			AddConnection(HouseLookup[houseFrom], HouseLookup[houseTo], intensity);
		}
		GD.Print("Sucesso em criar todas as conexões");
	}
}
