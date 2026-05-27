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
	[Export] public float RepulsionConstant = 800f;
	[Export] public float SpringConstant = 20f;
	[Export] public float RestDistance = 300f;
	[Export] public float Damping = 6f;
	[Export] public float MaxForce = 1200f;
	[Export] public float MinDistance = 80f;
	[Export] public float NegativeDistanceFactor = 1f;
	[Export] public float MaxAttractionDisplacement = 200f;

	public override void _Ready()
	{
		SetupInitialGreatHouses();
		SetupInitialLordlyHouses();
		SetupHousesLookup();

		Reasoner = new Reasoner(this);
		CreateRawConnections();
		SetupKingdomsRawConnections();
		
		// Debug: Verificar qual é a casa mais conectada após a criação do grafos
		GetMostConnectedHouse();
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

			CreateOrUpdateConnection(houseFrom, houseTo, intensity, true);
		}
	}

	public void InferKingdomConnections()
	{
		// O Corpo pergunta ao Cérebro quais são as conexões lógicas
		var implicitConnections = Reasoner.InferKingdomRules(Graph.Keys);

		foreach (var connection in implicitConnections)
		{
            double intensity = Reasoner.GetConnectionIntensity(new GCollections.Array<string> { connection.Type });

			CreateOrUpdateConnection(connection.From, connection.To, intensity, true);
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

	private void CreateOrUpdateConnection(string fromHouseName, string toHouseName, double intensity, bool updateScale)
	{
		if (!HouseLookup.ContainsKey(fromHouseName) || !HouseLookup.ContainsKey(toHouseName)) 
		{
			// Debug
			GD.PrintErr($"Aviso: Tentativa de conectar {fromHouseName} a {toHouseName}, mas uma das casas não existe no grafo.");
			return;
		}

		WesterosHouse fromHouse = HouseLookup[fromHouseName];
		WesterosHouse toHouse = HouseLookup[toHouseName];

		if (updateScale)
		{
			fromHouse.UpdateScale();
			toHouse.UpdateScale();
		}

		if (Graph[fromHouse].ContainsKey(toHouse))
		{
			Graph[fromHouse][toHouse].Intensity = intensity;
			Graph[toHouse][fromHouse].Intensity = intensity;
		}
		else
		{
			AddConnection(fromHouse, toHouse, intensity);
		}
	}

	private void ApplyAtractionForces()
	{
		// Forças de atração apenas acontecem entre casas conectadas. 
		// Casas não conectadas não se atraem e todas as casas se repelem por uma questão de repulsão natural.

		appliedForces.Clear();
		foreach (WesterosHouse from in Graph.Keys)
		{
			foreach (var to in Graph[from])
			{
				Tuple<WesterosHouse, WesterosHouse> forceTuple = new Tuple<WesterosHouse, WesterosHouse>(from, to.Key);
				Tuple<WesterosHouse, WesterosHouse> reverseForceTuple = new Tuple<WesterosHouse, WesterosHouse>(to.Key, from);

				if (appliedForces.Contains(forceTuple) || appliedForces.Contains(reverseForceTuple)) continue;
				
				Vector2 direction = to.Key.GlobalPosition - from.GlobalPosition;
				float distance = direction.Length();

				if (distance <= 0) continue;

				float intensity = (float)to.Value.Intensity;
				float weight = Mathf.Abs(intensity);
				if (weight <= 0.0001f) continue;

				float desiredDistance = RestDistance;
				if (intensity < 0f)
				{
					desiredDistance = RestDistance * (1f + NegativeDistanceFactor * weight);
				}

				float displacement = distance - desiredDistance;
				if (displacement > 0f)
				{
					displacement = Mathf.Min(displacement, MaxAttractionDisplacement);
				}
				float strength = SpringConstant * displacement * weight;
				strength = Mathf.Clamp(strength, -MaxForce, MaxForce);
				Vector2 force = direction.Normalized() * strength;

				from.ApplyCentralForce(force);
				to.Key.ApplyCentralForce(-force);

				appliedForces.Add(forceTuple);
			}
		}
	}

	private void ApplyRepulsionForces()
	{
		var houses = HouseLookup.Values; // Me dá nós da cena (casas) <string, Casas>
		var houseList = new List<WesterosHouse>(houses);

		for (int i = 0; i < houseList.Count; i++)
		{
			for (int j = i + 1; j < houseList.Count; j++)
			{
				WesterosHouse houseA = houseList[i];
				WesterosHouse houseB = houseList[j];

				Vector2 direction = houseA.GlobalPosition - houseB.GlobalPosition;
				float distance = direction.Length();

				if (distance <= 0) continue;

				float safeDistance = Mathf.Max(distance, MinDistance);
				float strength = RepulsionConstant / (safeDistance * safeDistance);
				strength = Mathf.Clamp(strength, 0f, MaxForce);
				Vector2 repulsionVector = direction.Normalized() * strength;

				houseA.ApplyCentralForce(repulsionVector);
				houseB.ApplyCentralForce(-repulsionVector);
			}
		}

		foreach (WesterosHouse house in houseList)
		{
			if (house is RigidBody2D body)
			{
				Vector2 dampingForce = -body.LinearVelocity * Damping;
				body.ApplyCentralForce(dampingForce);
			}
		}
	}

	public WesterosHouse GetMostConnectedHouse()
	{
		// Validação de integridade: previne erros se o método for chamado antes do grafo ser montado
		if (Graph == null || Graph.Count == 0)
		{
			GD.PrintErr("Aviso: Tentativa de buscar a casa mais conectada em um grafo vazio.");
			return null;
		}

		WesterosHouse mostConnectedHouse = null;
		int maxDegree = -1;

		foreach (var node in Graph)
		{
			// O grau do vértice é a quantidade de arestas (conexões) que ele possui
			int currentDegree = node.Value.Count;

			if (currentDegree > maxDegree)
			{
				maxDegree = currentDegree;
				mostConnectedHouse = node.Key;
			}
		}

		GD.Print($"A casa com maior grau é {mostConnectedHouse.Name} com {maxDegree} conexões.");
		return mostConnectedHouse;
	}

	public WesterosHouse GetMostWellConnectedHouse()
	{
		// Validação de segurança básica
		if (Graph == null || Graph.Count == 0)
		{
			GD.PrintErr("Aviso: Tentativa de buscar a casa mais bem conectada em um grafo vazio.");
			return null;
		}

		WesterosHouse mostWellConnectedHouse = null;
		
		// Inicializamos com o menor valor possível, pois a soma das intensidades 
		// pode ser negativa no escopo [-1, 1] caso a casa só tenha hostilidades.
		double maxIntensitySum = double.MinValue; 

		foreach (var node in Graph)
		{
			WesterosHouse currentHouse = node.Key;

			// Otimização de Domínio: LordlyHouses são ignoradas conceitualmente
			if (currentHouse is LordlyHouse)
			{
				continue;
			}

			double currentIntensitySum = 0;

			// Soma as intensidades de todas as arestas conectadas a esta casa
			foreach (var edge in node.Value.Values)
			{
				currentIntensitySum += edge.Intensity;
			}

			// Atualiza o recorde se a soma atual for estritamente maior
			if (currentIntensitySum > maxIntensitySum)
			{
				maxIntensitySum = currentIntensitySum;
				mostWellConnectedHouse = currentHouse;
			}
		}

		if (mostWellConnectedHouse != null)
		{
			GD.Print($"A casa mais BEM conectada é {mostWellConnectedHouse.Name} com intensidade acumulada de {maxIntensitySum}.");
		}

		return mostWellConnectedHouse;
	}
}
