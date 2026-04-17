using Godot;
using System;

public class InferredConnection
{
	public string From { get; set; }
	public string To { get; set; }
	public string Type { get; set; }

	public InferredConnection(string from, string type,  string to)
	{
		From = from;
		Type = type;
		To = to;
	}
}