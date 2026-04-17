using Godot;
using GCollections = Godot.Collections;
using System.Collections.Generic;
using System;

public partial class GraphController : Node2D
{
	protected Dictionary<WesterosHouse, Dictionary<WesterosHouse, Edge>> Graph { get; private set; } = new Dictionary<WesterosHouse, Dictionary<WesterosHouse, Edge>>();
	protected Dictionary<string, WesterosHouse> HouseLookup { get; private set; } = new Dictionary<string, WesterosHouse>();
	protected DataManager dataManager = new DataManager();
	protected HashSet<Tuple<WesterosHouse, WesterosHouse>> drawnConnections = new HashSet<Tuple<WesterosHouse, WesterosHouse>>();
	protected HashSet<Tuple<WesterosHouse, WesterosHouse>> appliedForces = new HashSet<Tuple<WesterosHouse, WesterosHouse>>();
	protected Reasoner Reasoner { get; set;}
	protected const float BASE_CONECTION_WIDTH = 12;
	[Export] public float RepulsionConstant = 8000f;
	[Export] public float SpringConstant = 2f;
	[Export] public float RestDistance = 300f; //Dnew GCollections.Array<string>() {"isVassalOf"}istância em que as casas repousam/param de se atrair

	public override void _Ready()
	{
		SetupInitialGreatHouses();
		SetupInitialLordlyHouses();
		SetupHousesLookup();
		Reasoner = new Reasoner(this);
		SetupGreatHousesConnections();
		SetupKingdomsConnections();
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
		foreach (WesterosHouse from in Graph.Keys)
		{
			foreach (var to in Graph[from])
			{
				Tuple<WesterosHouse, WesterosHouse> connectionTuple = new Tuple<WesterosHouse, WesterosHouse>(from, to.Key);
				
				// Retorna caso ja tenhamos desenhado a relação casa A casa B, para evitar desenhar casa B com casa A
				if (drawnConnections.Contains(connectionTuple)) continue; 
				
				Color connectionColor = GetConnectionColor(to.Value.Intensity);

				DrawLine(from.GlobalPosition, to.Key.GlobalPosition, connectionColor, BASE_CONECTION_WIDTH * (float)to.Value.Intensity, true);
				drawnConnections.Add(connectionTuple);
			}
		}
    }

	private void AddConnection(WesterosHouse from, WesterosHouse to, double intensity)
	{
		if (!Graph.ContainsKey(from) || !Graph.ContainsKey(to)) return;
		
		Graph[from].Add(to, new Edge(intensity));
		Graph[to].Add(from, new Edge(intensity));
	}

	private void SetupInitialGreatHouses()
	{
		// Aqui estamos pegando as referências de nós da cena, nesse caso, 
		// cada nó desse tá como uma GreatHouse, por isso fazemose esse Get<GreatHouse>
		var GreatHousesNames = (GCollections.Array<string>)dataManager.GetDataFromJson("init", "GreatHouses");
		var GreatHousesData = (GCollections.Dictionary)dataManager.GetDataFromJson("init", "GreatHousesData");
		float defaultSize = (float)GreatHousesData["Size"];

		foreach (string greatHouse in GreatHousesNames)
		{
			GreatHouse greatHouseNode = GetNode<GreatHouse>(greatHouse);
			GCollections.Dictionary houseData = (GCollections.Dictionary)GreatHousesData[greatHouse];
			
			greatHouseNode.Size = defaultSize;
			greatHouseNode.Rules = (string) houseData["Rules"];
			greatHouseNode.IsLoyalToTheCrown = (bool) houseData["IsLoyalToTheCrown"];
			
			greatHouseNode.UpdateScale();
			Graph.Add(greatHouseNode, new Dictionary<WesterosHouse, Edge>());

			//GD.Print($"Adicionando {greatHouseNode.Name} ao grafo com tamanho {greatHouseNode.Size} e facção {greatHouseNode.Rules} que é leal à coroa? {greatHouseNode.IsLoyalToTheCrown}");
		}

		GD.Print("Great Houses iniciadas com sucesso");
	}

	private void SetupInitialLordlyHouses()
	{
		var Kingdoms = (GCollections.Array<string>)dataManager.GetDataFromJson("init", "Kingdoms");
		var LordlyHousesData = (GCollections.Dictionary)dataManager.GetDataFromJson("init", "LordlyHousesData");
		float defaultSize = (float)LordlyHousesData["Size"];
		
		foreach (string kingdom in Kingdoms)
		{
			GCollections.Dictionary kingdomData = (GCollections.Dictionary) LordlyHousesData[kingdom];
			GCollections.Array LordlyHouses = (GCollections.Array) kingdomData["Names"];
			
			string defaultVassalOf = (string) kingdomData["VassalOf"];
			
			foreach (string lordlyhouse in LordlyHouses)
			{
				LordlyHouse lordlyHouseNode = GetNode<LordlyHouse>(lordlyhouse);

				lordlyHouseNode.Size = defaultSize;
				lordlyHouseNode.VassalOf = defaultVassalOf;
				lordlyHouseNode.Faction = kingdom;

				lordlyHouseNode.UpdateScale();
				Graph.Add(lordlyHouseNode, new Dictionary<WesterosHouse, Edge>());
				
				//GD.Print($"Adicionando LordlyHouse {lordlyHouseNode.Name} ao grafo com tamanho {lordlyHouseNode.Size} facção {lordlyHouseNode.Faction} leal a {lordlyHouseNode.VassalOf}");
			}
		}

		GD.Print("LordlyHouses adicionadas ao grafo");
	}

	private void SetupHousesLookup()
	{
		// Populamos o dicionário de lookup para termos a referência de cada casa a partir do nome.
		// O objetivo é ter a referência do nó a partir do nome, necessário para criação de conexões em SetupInitialRelations() e outras mudanças futuras
		foreach (var house in Graph.Keys)
		{
			HouseLookup[house.Name] = house;
		}
	}

	private void SetupGreatHousesConnections()
	{
		GCollections.Array<Variant> InitialConections = (GCollections.Array<Variant>)dataManager.GetDataFromJson("init","GreatHousesConnections");
		
		foreach (Variant Connection in InitialConections)
		{
			GCollections.Dictionary ConnectionAsDict = (GCollections.Dictionary)Connection;
			GCollections.Array<string> connectionTypes = (GCollections.Array<string>)ConnectionAsDict["ConnectionTypes"];

			string houseFrom = (string)ConnectionAsDict["from"];
			string houseTo = (string)ConnectionAsDict["to"];			
			float intensity = Reasoner.GetConnectionIntensity(connectionTypes);
			
			AddConnection(HouseLookup[houseFrom], HouseLookup[houseTo], intensity);
		}
	}

	private void SetupKingdomsConnections()
	{
		// O Corpo pergunta ao Cérebro quais são as conexões lógicas
		var implicitConnections = Reasoner.InferKingdomRules(Graph.Keys);

		foreach (var connection in implicitConnections)
		{
            float intensity = Reasoner.GetConnectionIntensity(new GCollections.Array<string> { connection.Type });
			AddConnection(HouseLookup[connection.From], HouseLookup[connection.To], intensity);
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
		foreach(WesterosHouse from in Graph.Keys)
		{
			foreach (var to in Graph[from])
			{
				Tuple<WesterosHouse, WesterosHouse> forceTuple = new Tuple<WesterosHouse, WesterosHouse>(from, to.Key);

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

		foreach (WesterosHouse houseA in greatHouses)
		{
			foreach (WesterosHouse houseB in greatHouses)
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
