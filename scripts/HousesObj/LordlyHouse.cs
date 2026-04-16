using Godot;

[GlobalClass]
public partial class LordlyHouse : WesterosHouse
{
    // Propriedade de Objeto: isVassalOf -> GreatHouse
    public string VassalOf { get; set; }
    public string Faction { get; set; }
}