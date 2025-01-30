using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class CraftingItemSlotComponent : ItemSlotComponent
{
    private readonly Color COLOR_WHITE = new Color(1, 1, 1, 1);
    private readonly Color COLOR_TRANSPARENT = new Color(1, 1, 1, 0.5f);

    private CraftingRecipe recipe;
    private HashSet<InventoryItemDefinition> recipeItemTypes = new();

    public override void _Ready()
    {
        base._Ready();

        ServiceLocator.InventoryService.OnItemAdded += OnInventoryUpdateItem;
        ServiceLocator.InventoryService.OnItemRemoved += OnInventoryUpdateItem;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        ServiceLocator.InventoryService.OnItemAdded -= OnInventoryUpdateItem;
        ServiceLocator.InventoryService.OnItemRemoved -= OnInventoryUpdateItem;
    }

    public void SetRecipe(CraftingRecipe recipe)
    {
        this.recipe = recipe;

        foreach (CraftingRecipeItem recipeItem in recipe.requiredItems)
        {
            recipeItemTypes.Add(recipeItem.item);
        }

        SetItem(recipe.result.item, recipe.result.count);

        UpdateCanCraftState();
    }

    private void OnInventoryUpdateItem(InventoryItem item)
    {
        if(recipeItemTypes.Contains(item.definition))
        {
            UpdateCanCraftState();
        }
    }

    private void UpdateCanCraftState()
    {
        bool canCraft = ServiceLocator.InventoryService.CanCraftRecipe(recipe);
        Modulate = canCraft ? COLOR_WHITE : COLOR_TRANSPARENT;
    }
}