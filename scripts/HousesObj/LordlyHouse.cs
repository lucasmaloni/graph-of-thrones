using Godot;

[GlobalClass]
public partial class LordlyHouse : WesterosHouse
{
    // Propriedade de Objeto: isVassalOf -> GreatHouse
    public GreatHouse VassalOf { get; set; }

    // O "Cérebro" usará isso para inferir lealdade automática
    public bool CheckLoyaltyInheritance()
    {
        return VassalOf != null && VassalOf.IsLoyalToTheCrown;
    }
}