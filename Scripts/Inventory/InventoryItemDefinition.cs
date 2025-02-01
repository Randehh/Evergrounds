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
    public InventoryItemRarity rarity;

    [Export]
    public float worldShadowScale = 0.5f;

    [Export(hintString: "Defines what is placed on the grid with grid-based interaction types")]
    public string gridPlacementScene;

    [Export(hintString: "Defines which material to change the tile to in tile material events, or on which tile the item can be placed")]
    public AtlasMaterial tileMaterial;

    [Export]
    public Vector2I subTileSet = Vector2I.Zero;

    [Export]
    public bool isSimpleTileMap = false;

    public bool isStackable => stackSize > 1;

    public InventoryItemDefinition() { }
}
