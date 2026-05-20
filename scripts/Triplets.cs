using Godot;
using System;

public class Triplets
{
	public string From { get; set; }
	public string To { get; set; }
	public string Type { get; set; }

	public Triplets(string from, string type,  string to)
	{
		From = from;
		Type = type;
		To = to;
	}
}