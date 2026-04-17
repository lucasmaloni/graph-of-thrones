using Godot;

[GlobalClass]
public partial class GreatHouse : WesterosHouse
{
    // Propriedade de Objeto: rules -> Kingdom
    public string Rules { get; set; }
    
    // Atributo semantico de estado
    public bool IsLoyalToTheCrown { get; set; }
}