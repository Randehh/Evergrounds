using Godot;

[GlobalClass]
public partial class InteractEventOpenCrafting : InteractEvent
{
    [Export]
    private CraftingRecipeContainer recipeContainer;

    public override void Execute()
    {
        TabUI.Instance.ShowShopUI(recipeContainer);
    }
}