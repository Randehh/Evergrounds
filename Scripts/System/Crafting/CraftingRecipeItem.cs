using Godot;

[GlobalClass]
public partial class CraftingRecipeItem : Resource
{

    [Export]
    public InventoryItemDefinition item;

    [Export]
    public int count = 1;
}
