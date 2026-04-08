using Godot;
using Godot.Collections;

public partial class DataManager : Node
{
	public string DataFilePath {get; private set;} = "res://scripts/data/database.json";
	
	public Array<Variant> GetDataFromJson(string keyFromJson)
	{
		if (!FileAccess.FileExists(DataFilePath))
    	{
        	GD.PrintErr($"Arquivo não encontrado: {DataFilePath}");
        	return new Array<Variant>();
		}

		using var file = FileAccess.Open(DataFilePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Variant jsonVariant = Json.ParseString(jsonString);
		Dictionary fullData = (Dictionary)jsonVariant;

		if (fullData.ContainsKey(keyFromJson))
		{
			// Fazemos casting de Array<Variant> para garantir o retorno correto
			return (Array<Variant>)fullData[keyFromJson];
		}
		else
		{
			GD.Print($"{keyFromJson} não existe no arquivo {DataFilePath}");
			return new Array<Variant>();
		}
	}
}
