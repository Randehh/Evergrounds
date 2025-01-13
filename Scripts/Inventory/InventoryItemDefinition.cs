using Godot;
using static CharacterBase;

[GlobalClass]
public partial class InventoryItemDefinition : Resource
{
    [Export]
    public InventoryItemType itemType;

    [Export]
    public Texture2D itemSprite;

    [Export]
    public int stackSize = 1;

    [Export]
    public HoldableAnimations useAnimation;

    [Export]
    public InteractType interactType;

    [Export]
    public int interactLevel = 1;

    [Export]
    public float worldShadowScale = 0.5f;

    [Export(hintString: "Defines what is placed on the grid with grid-based interaction types")]
    public string gridPlacementScene;

    public bool isStackable => stackSize > 1;

    public InventoryItemDefinition() { }
}
