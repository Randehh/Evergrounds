using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CraftingRecipeContainer : Resource
{

    [Export]
    public Array<CraftingRecipe> recipes = new ();
}
