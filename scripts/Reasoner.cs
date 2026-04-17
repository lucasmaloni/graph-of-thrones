using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Reasoner
{
	private Dictionary<string, float> ConnectionLookup { get; set; }
	private GraphController GraphController { get; set; }

	public Reasoner (GraphController graphController)
	{
		GraphController = graphController;
		ConnectionLookup = SeupConnectionsLookup();
	}
	private  Dictionary<string, float> SeupConnectionsLookup()
	{
		return new Dictionary<string, float>
		{
			{ "isVassalOf", 0.5f },
			{ "isMarriedTo", 1.0f },
			{ "isHistoricalFriendTo", 0.75f },
			{ "isEstrategicalAllyTo", 0.25f },
			{ "isFamilyOf", 0.5f }
		};
	}

	public float GetConnectionIntensity(Godot.Collections.Array<string> connectionTypes)
	{
		float intensity = 0f;
		foreach (string connectionType in connectionTypes)
		{
			if (ConnectionLookup.ContainsKey(connectionType))
			{
				intensity += ConnectionLookup[connectionType];
			}
		}

		if (intensity > 1f) intensity = 1f;

		return intensity;
	}

	public List<InferredConnection> InferKingdomRules(IEnumerable<WesterosHouse> allHouses)
	{
		var inferred = new List<InferredConnection>();
		
		foreach (var house in allHouses)
		{
			// Regra Semântica: Se é LordlyHouse, deve haver um link de vassalagem
			if (house is LordlyHouse lordly && !string.IsNullOrEmpty(lordly.VassalOf))
			{
				inferred.Add(new InferredConnection (
					lordly.HouseName,
					"isVassalOf",
					lordly.VassalOf
				));
			}
		}
		return inferred;
	}
}
