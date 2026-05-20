using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Reasoner
{
	private Dictionary<string, double> ConnectionLookup { get; set; }
	private Dictionary<string, Tuple<double, bool>> ActionLookup { get; set; }
	private GraphController GraphController { get; set; }

	public Reasoner (GraphController graphController)
	{
		GraphController = graphController;
		ConnectionLookup = SeupConnectionsLookup();
		ActionLookup = SetupActionsLookup();
	}
	private  Dictionary<string, double> SeupConnectionsLookup()
	{
		return new Dictionary<string, double>
		{
			{ "isVassalOf", 0.5 },
			{ "isMarriedTo", 1.0 },
			{ "isHistoricalFriendTo", 0.75 },
			{ "isEstrategicalAllyTo", 0.25 },
			{ "isFamilyOf", 0.5 }
		};
	}

	private Dictionary<string, Tuple<double, bool>> SetupActionsLookup()
	{
		// Relação ação e decremento de intensidade associado e se é uma ação que afeta redes adjacentes
		// Precisa de validação para garantir que as ações estão de acordo com o universo de Game of Thrones
		return new Dictionary<string, Tuple< double, bool>>()
		{
			{ "Betrays", new Tuple<double, bool>(-1.0, false) },
			{ "FormPoliticalAllianceWith", new Tuple<double, bool>(+0.5, false) },
			{ "Marries", new Tuple<double, bool>(+1.0, false) },
			{ "KillsLeaderOf", new Tuple<double, bool>(-1.0, true) },
			{ "Supports", new Tuple<double, bool>(+0.75, false) },
			{ "BreaksCustomsWith", new Tuple<double, bool>(-0.5, true) }
		};
	}

	public double GetConnectionIntensity(Godot.Collections.Array<string> connectionTypes)
	{
		double intensity = 0;
		foreach (string connectionType in connectionTypes)
		{
			if (ConnectionLookup.ContainsKey(connectionType))
			{
				intensity += ConnectionLookup[connectionType];
			}
		}

		if (intensity > 1) intensity = 1;

		return intensity;
	}

	public List<Triplets> InferKingdomRules(IEnumerable<WesterosHouse> allHouses)
	{
		var inferred = new List<Triplets>();
		
		foreach (var house in allHouses)
		{
			// Regra Semântica: Se é LordlyHouse, deve haver um link de vassalagem
			if (house is LordlyHouse lordly && !string.IsNullOrEmpty(lordly.VassalOf))
			{
				inferred.Add(new Triplets (
					lordly.HouseName,
					"isVassalOf",
					lordly.VassalOf
				));
			}
		}
		return inferred;
	}

	public void ProcessAction(Triplets actionTriplet)
	{
		if (ActionLookup.ContainsKey(actionTriplet.Type))
		{
			var (intensityChange, affectsAdjacent) = ActionLookup[actionTriplet.Type];
			// Lógica de mudança de intensidade entre conexões no grafo

		}
	}
}
