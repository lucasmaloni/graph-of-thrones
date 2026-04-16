using Godot;
using Godot.Collections;

public partial class DataManager : Node
{
	public string DataFilePath {get; private set;}
	
	public Variant GetDataFromJson(string file, string keyFromJson)
	{
		string DataFilePath = $"res://scripts/data/{file}.json";
		if (!FileAccess.FileExists(DataFilePath))
    	{
        	GD.PrintErr($"Arquivo não encontrado: {DataFilePath}");
        	return new Array<Variant>();
		}

		using var fileContent = FileAccess.Open(DataFilePath, FileAccess.ModeFlags.Read);
		string jsonString = fileContent.GetAsText();
		Variant jsonVariant = Json.ParseString(jsonString);
		Dictionary fullData = (Dictionary)jsonVariant;

		if (fullData.ContainsKey(keyFromJson))
		{
			return fullData[keyFromJson];
		}
		else
		{
			GD.Print($"{keyFromJson} não existe no arquivo {DataFilePath}");
			return new Array<Variant>();
		}
	}
}
