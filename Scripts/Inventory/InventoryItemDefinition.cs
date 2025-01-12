using Godot;

[GlobalClass]
public partial class InventoryItemDefinition : Resource
{
    [Export]
    public InventoryItemType itemType;

    [Export]
    public Texture2D itemSprite;

    [Export]
    public int stackSize = 1;

    public bool isStackable => stackSize > 1;

    public InventoryItemDefinition() { }
}
