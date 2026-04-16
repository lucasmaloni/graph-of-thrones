using Godot;

public partial class WesterosHouse : RigidBody2D
{
    [Export] public string HouseName { get; set; } = string.Empty;
    [Export] public float Size { get; set; } = 1.0f;
    
    // Dimensões padrão para manter a proporção (x, 1.1x)
    public Vector2 TargetDimensions { get; set; } = new Vector2(128, 141);
    public Sprite2D SigilSprite { get; private set; }

    public override void _Ready()
    {
        SigilSprite = GetNodeOrNull<Sprite2D>("Sigil");
        if (SigilSprite == null)
        {
            GD.PushError($"No '{nameof(Sprite2D)}' node named 'Sigil' in '{Name}'.");
            return;
        }

        // If the scene does not set HouseName explicitly, use the node's name.
        if (string.IsNullOrWhiteSpace(HouseName))
        {
            HouseName = Name.ToString();
        }
        
        // Configurações físicas globais da ontologia (TBox)
        GravityScale = 0; 
        LinearDamp = 10.0f; 
        LockRotation = true;

        ConfigureHouse();
    }

    private void ConfigureHouse()
    {
        if (SigilSprite == null || string.IsNullOrWhiteSpace(HouseName)) return;

        string path = $"res://assets/icons/{HouseName}.svg";
        if (ResourceLoader.Exists(path))
        {
            SigilSprite.Texture = GD.Load<Texture2D>(path);
            UpdateScale();
        }
        else
        {
            GD.PushWarning($"Sigil not found for house '{HouseName}' at '{path}'.");
        }
    }

    public void UpdateScale()
    {
        if (SigilSprite?.Texture != null)
        {
            Vector2 textureDimension = SigilSprite.Texture.GetSize();
            float effectiveSize = Size > 0 ? Size : 1.0f;
            SigilSprite.Scale = TargetDimensions / textureDimension * effectiveSize;
        }
    }
}
