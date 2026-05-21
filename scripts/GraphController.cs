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
	protected const float BASE_CONECTION_WIDTH = 3f;
	[Export] public float RepulsionConstant = 8000f;
	[Export] public float SpringConstant = 200f;
	[Export] public float RestDistance = 300f; //Dnew GCollections.Array<string>() {"eVassaloDe"}istância em que as casas repousam/param de se atrair

	public override void _Ready()
	{
		SetupInitialGreatHouses();
		SetupInitialLordlyHouses();
		SetupHousesLookup();

		Reasoner = new Reasoner(this);
		CreateRawConnections();
		SetupKingdomsRawConnections();
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

				DrawLine(from.GlobalPosition, to.Key.GlobalPosition, connectionColor, BASE_CONECTION_WIDTH, true);
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

				Graph.Add(lordlyHouseNode, new Dictionary<WesterosHouse, Edge>());
				
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

	private bool TryUpdateExistingConnection(string fromHouseName, string toHouseName, double intensity, bool updateScale)
	{
		if (!HouseLookup.ContainsKey(fromHouseName) || !HouseLookup.ContainsKey(toHouseName)) return false;
		WesterosHouse fromHouse = HouseLookup[fromHouseName];
		WesterosHouse toHouse = HouseLookup[toHouseName];

		if (updateScale)
		{
			fromHouse.UpdateScale();
			toHouse.UpdateScale();
		}

		if (!Graph[fromHouse].ContainsKey(toHouse)) return false;

		Graph[fromHouse][toHouse].Intensity = intensity;
		Graph[toHouse][fromHouse].Intensity = intensity;
		return true;
	}

	public void InferGreatHousesConnections()
	{
		GCollections.Array<Variant> InitialConections = (GCollections.Array<Variant>)dataManager.GetDataFromJson("init","GreatHousesConnections");
		
		foreach (Variant Connection in InitialConections)
		{
			GCollections.Dictionary ConnectionAsDict = (GCollections.Dictionary)Connection;
			GCollections.Array<string> connectionTypes = (GCollections.Array<string>)ConnectionAsDict["ConnectionTypes"];

			string houseFrom = (string)ConnectionAsDict["from"];
			string houseTo = (string)ConnectionAsDict["to"];			
			double intensity = Reasoner.GetConnectionIntensity(connectionTypes);

			TryUpdateExistingConnection(houseFrom, houseTo, intensity, true);
		}
	}

	public void InferKingdomConnections()
	{
		// O Corpo pergunta ao Cérebro quais são as conexões lógicas
		var implicitConnections = Reasoner.InferKingdomRules(Graph.Keys);

		foreach (var connection in implicitConnections)
		{
            double intensity = Reasoner.GetConnectionIntensity(new GCollections.Array<string> { connection.Type });

			TryUpdateExistingConnection(connection.From, connection.To, intensity, true);
		}
	}

	private void SetupKingdomsRawConnections()
	{
		if (Reasoner == null) return;

		var implicitConnections = Reasoner.InferKingdomRules(Graph.Keys);

		foreach (var connection in implicitConnections)
		{
			AddConnection(HouseLookup[connection.From], HouseLookup[connection.To], 0.0);
		}
	}
	
	private void CreateRawConnections()
	{
		var rawConnections = (GCollections.Array<Variant>)dataManager.GetDataFromJson("raw_data", "RawConnections");

		foreach (Variant connection in rawConnections)
		{
			GCollections.Dictionary connectionAsDict = (GCollections.Dictionary)connection;
			string from = (string)connectionAsDict["from"];
			string to = (string)connectionAsDict["to"];
			double intensity = (double)connectionAsDict["intensity"];

			if (!HouseLookup.ContainsKey(from) || !HouseLookup.ContainsKey(to)) continue;
			if (Graph[HouseLookup[from]].ContainsKey(HouseLookup[to])) continue;

			AddConnection(HouseLookup[from], HouseLookup[to], intensity);
		}
	}

	private Color GetConnectionColor(double intensity)
{   
    // Clamp para garantir que o valor não fuja do intervalo [-1.0, 1.0]
    float val = Mathf.Clamp((float)intensity, -1.0f, 1.0f);

    // Definição das cores base
    Color colorRed   = new Color(1.0f, 0.1f, 0.1f, 1.0f); // -1.0: Hostilidade (Vermelho)
    Color colorWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f); //  0.0: Neutralidade (Branco)
    Color colorBlue  = new Color(0.1f, 0.1f, 1.0f, 1.0f); //  1.0: Aliança (Azul)

    if (val < 0.0f)
    {
		// Interpolação de Vermelho para Branco (-1.0 até 0.0)
        float t = val + 1.0f; 
        return colorRed.Lerp(colorWhite, t);
    }
    else
    {
        // Interpolação de Branco para Azul (0.0 até 1.0)
        float t = val;
        return colorWhite.Lerp(colorBlue, t);
    }
}

	public void UpdateConnection(string fromHouseName, string toHouseName, double intensityChange)
	{
		if (!HouseLookup.ContainsKey(fromHouseName) || !HouseLookup.ContainsKey(toHouseName)) return;

		WesterosHouse fromHouse = HouseLookup[fromHouseName];
		WesterosHouse toHouse = HouseLookup[toHouseName];

		if (intensityChange < 0)
		{
			double currentIntensity = Graph[fromHouse][toHouse].Intensity;
			double newIntensity = Math.Max(0, currentIntensity + intensityChange); // Garante

			if (newIntensity == 0)
			{
				Graph[fromHouse].Remove(toHouse);
				Graph[toHouse].Remove(fromHouse);
			}
			else
			{
				Graph[fromHouse][toHouse].Intensity = newIntensity;
				Graph[toHouse][fromHouse].Intensity = newIntensity;
			}
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
				Tuple<WesterosHouse, WesterosHouse> reverseForceTuple = new Tuple<WesterosHouse, WesterosHouse>(to.Key, from);

				if (appliedForces.Contains(forceTuple) || appliedForces.Contains(reverseForceTuple)) continue; // Evita aplicar força mais de uma vez para a mesma conexão
				
				Vector2 direction = to.Key.GlobalPosition - from.GlobalPosition;
				float distance = direction.Length();

				if (distance <= 0) continue; // Evita divisão por zero

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

				if (distance <= 0) continue;

				float Strength = RepulsionConstant / (distance * distance);
				Vector2 repulsionVector = direction.Normalized() * Strength;

				houseA.ApplyCentralForce(repulsionVector);
            	houseB.ApplyCentralForce(-repulsionVector);
			}
		}
	}
}
