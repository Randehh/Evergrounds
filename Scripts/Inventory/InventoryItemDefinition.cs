using Godot;
using Godot.Collections;
using static CharacterBase;
using static WorldMap;

[GlobalClass]
public partial class InventoryItemDefinition : Resource
{
    [Export]
    public string displayName;

    [Export(PropertyHint.MultilineText)]
    public string description;

    [Export]
    public string usageText;

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

    [Export(hintString: "Defines which material to change the tile to in tile material events, or on which tile the item can be placed")]
    public AtlasMaterial tileMaterial;

    [Export(hintString: "Defines on which tiles the item can be placed")]
    public Array<AtlasMaterial> validTilePlacementMaterials = new();

    public bool isStackable => stackSize > 1;

    public InventoryItemDefinition() { }
}
