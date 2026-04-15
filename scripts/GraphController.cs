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
	protected HashSet<Tuple<GreatHouse, GreatHouse>> appliedForces = new HashSet<Tuple<GreatHouse, GreatHouse>>();
	protected const float BASE_CONECTION_WIDTH = 12;

	[Export] public float RepulsionConstant = 8000f;
	[Export] public float SpringConstant = 2f;
	[Export] public float RestDistance = 300f; //Distância em que as casas repousam/param de se atrair

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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
		ApplyAtractionForces();
		ApplyRepulsionForces();
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
				
				Color connectionColor = GetConnectionColor(to.Value.Intensity);

				DrawLine(from.GlobalPosition, to.Key.GlobalPosition, connectionColor, BASE_CONECTION_WIDTH * (float)to.Value.Intensity, true);
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
		var AllHousesData = (GCollections.Dictionary)dataManager.GetDataFromJson("GreatHouses");

		foreach (var greatHouse in AllHousesData)
		{
			string houseName = (string)greatHouse.Key;
			GreatHouse greatHouseNode = GetNode<GreatHouse>(houseName);

			GCollections.Dictionary houseData = (GCollections.Dictionary)greatHouse.Value;
			float houseSize = (float) houseData["houseSize"];
			string houseFaction = (string) houseData["faction"];

			greatHouseNode.HouseSize = houseSize;
			greatHouseNode.Faction = houseFaction;
			greatHouseNode.UpdateScale();

			GD.Print($"Adicionando {greatHouseNode.HouseName} ao grafo com tamanho {greatHouseNode.HouseSize} e facção {greatHouseNode.Faction}");
			Graph.Add(greatHouseNode, new Dictionary<GreatHouse, Edge>());
		}
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
		GCollections.Array<Variant> InitialConections = (GCollections.Array<Variant>)dataManager.GetDataFromJson("InitialConnections");

		foreach (Variant Connection in InitialConections)
		{
			GCollections.Dictionary ConnectionAsDict = (GCollections.Dictionary)Connection;

			string houseFrom = (string)ConnectionAsDict["from"];
			string houseTo = (string)ConnectionAsDict["to"];
			double intensity = (double)ConnectionAsDict["intensity"];

			AddConnection(HouseLookup[houseFrom], HouseLookup[houseTo], intensity);
		}
	}

	private Color GetConnectionColor(double intensity)
	{	
		float t = Mathf.Clamp((float)intensity, 0.0f, 1.0f);
    
		Color lowIntensityColor = new Color(1.0f, 0.1f, 0.1f, 1.0f);    // Vermelho suave
		Color midIntensityColor = new Color(0.1f, 1.0f, 0.1f, 1.0f);    // Verde
		Color highIntensityColor = new Color(0.1f, 0.1f, 1.0f, 1.0f);   // Azul suave

		if (t < 0.5f)
		{
			// Interpolação de Vermelho para Amarelo (0.0 a 0.5)
			return lowIntensityColor.Lerp(midIntensityColor, t * 2.0f);
		}
		else
		{
			// Interpolação de Verde para Azul (0.5 a 1.0)
			return midIntensityColor.Lerp(highIntensityColor, (t - 0.5f) * 2.0f);
		}
	}

	private void ApplyAtractionForces()
	{
		// Forças de atração apenas acontecem entre casas conectadas. 
		// Casas não conectadas não se atraem e todas as casas se repelem por uma questão de repulsão natural.

		appliedForces.Clear();
		foreach(GreatHouse from in Graph.Keys)
		{
			foreach (var to in Graph[from])
			{
				Tuple<GreatHouse, GreatHouse> forceTuple = new Tuple<GreatHouse, GreatHouse>(from, to.Key);

				if (appliedForces.Contains(forceTuple)) continue; // Evita aplicar força mais de uma vez para a mesma conexão
				
				Vector2 direction = to.Key.GlobalPosition - from.GlobalPosition;
				float distance = direction.Length();

				if (distance <= RestDistance) continue; // Evita divisão por zero

				float displacement = distance - RestDistance;
				float Strength = SpringConstant * displacement * (float)to.Value.Intensity;
				Vector2 force = direction.Normalized() * Strength;

				from.ApplyCentralForce(force);

				appliedForces.Add(forceTuple);
			}
		}
	}

	private void ApplyRepulsionForces()
	{
		var greatHouses = HouseLookup.Values; // Me dá nós da cena (casas) <string, Casas>

		foreach (GreatHouse houseA in greatHouses)
		{
			foreach (GreatHouse houseB in greatHouses)
			{
				if (houseA == houseB) continue; // Evita aplicar força em si mesmo

				Vector2 direction = houseA.GlobalPosition - houseB.GlobalPosition;
				float distance = direction.Length();

				if (distance <= RestDistance) continue;

				float Strength = RepulsionConstant / (distance * distance);
				Vector2 repulsionVector = direction.Normalized() * Strength;

				houseA.ApplyCentralForce(repulsionVector);
            	houseB.ApplyCentralForce(-repulsionVector);
			}
		}
		
	}
}
