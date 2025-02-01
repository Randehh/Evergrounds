using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CraftingRecipe : Resource
{
    [Export]
    public CraftingRecipeItem result;

    [Export]
    public Array<CraftingRecipeItem> requiredItems = new();

    [Export]
    public CraftingRecipeType recipeType;
}
