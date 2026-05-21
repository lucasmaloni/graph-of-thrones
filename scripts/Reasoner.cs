using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class Reasoner
{
	private Dictionary<string, double> ConnectionLookup { get; set; }
	private GraphController GraphController { get; set; }

	public Reasoner (GraphController graphController)
	{
		GraphController = graphController;
		ConnectionLookup = SetupConnectionsLookup();
	}
	private  Dictionary<string, double> SetupConnectionsLookup()
	{
		return new Dictionary<string, double>
		{
			// Instâncias semanticas de Tipos de Conexões
			{ "eVassaloDe", 0.5 },
			{ "eCasadoCom", 1.0 },
			{ "aliadoHistoricoDe", 0.75 },
			{ "eAliadoEstrategicoDe", 0.25 },
			{ "eFamiliaDe", 0.5 },
			{ "naoConfiaEm", -0.5 },
			{ "guardaRancorDe", -0.75 },
			{ "eInimigoDe", -1.0 }
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
					lordly.HouseName, // Casa que é vassala (Sujeito)
					"eVassaloDe", // Tipo de conexão (Predicado)
					lordly.VassalOf // Casa que é suserana (Objeto)
				));
			}
		}
		return inferred;
	}

}
