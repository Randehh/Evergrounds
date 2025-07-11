using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class CraftingUI : Control
{
    private readonly Color COLOR_WHITE = new Color(1, 1, 1, 1);
    private readonly Color COLOR_TRANSPARENT = new Color(1, 1, 1, 0.5f);

    [Export]
    private PackedScene inventorySlotScene;

    [Export]
    private PackedScene craftingComponentScene;

    [Export]
    private PackedScene craftingCategory;

    [Export]
    private VBoxContainer craftingListParent;

    [Export]
    private Label recipeTitleLabel;

    [Export]
    private Label recipeDescriptionLabel;

    [Export]
    private Label recipeComponentsLabel;

    [Export]
    private VBoxContainer recipeComponentContainer;

    [Export]
    private Button craftButton;

    [Export]
    private ExperienceDisplay experienceDisplay;

    [Export]
    private CraftingRecipeContainer defaultCraftingRecipeContainer;

    [Export]
    private string craftButtonText;

    [Export]
    private string componentsText;

    private InventoryService inventory;
    private CraftingRecipeContainer recipeContainer;
    private int slotMouseOver = -1;
    private List<CraftingItemSlotComponent> inventoryButtons = new();
    private int currentlySelected = -1;
    private CraftingRecipe[] sortedRecipes;

    public override void _Ready()
    {
        inventory = ServiceLocator.InventoryService;
        recipeContainer = defaultCraftingRecipeContainer;

        craftButton.Pressed += CraftButton_Pressed;

        ReloadUI();

        inventory.OnUpdateSlot += (_) =>
        {
            SetSelectedCraftingSlot(currentlySelected);
        };

        recipeComponentsLabel.Text = componentsText;
        craftButton.Text = craftButtonText;
    }   

    private void CraftButton_Pressed()
    {
        CraftingRecipe recipe = sortedRecipes[currentlySelected];
        foreach (var item in recipe.requiredItems)
        {
            inventory.RemoveItem(item.item, item.count);
        }

        inventory.AddItem(recipe.result.item, recipe.result.count);

        ServiceLocator.ExperienceService.AddExperience(ExperienceType.CRAFTING, recipe.experience);
    }

    public void SetCraftingContainer(CraftingRecipeContainer craftingRecipeContainer)
    {
        recipeContainer = craftingRecipeContainer;
        ReloadUI();

        experienceDisplay.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustReleased("click"))
        {
            if(slotMouseOver != -1)
            {
                SetSelectedCraftingSlot(slotMouseOver);
            }
        }
    }

    private void ReloadUI()
    {
        inventoryButtons.Clear();

        foreach(var child in craftingListParent.GetChildren())
        {
            craftingListParent.RemoveChild(child);
            child.Free();
        }

        sortedRecipes = recipeContainer.recipes.OrderBy(recipe => recipe.recipeType).ThenBy(recipe => recipe.result.item.displayName).ToArray();
        CraftingRecipeType? lastRecipeType = null;
        FlowContainer currentFlowContainer = null;

        for (int i = 0; i < sortedRecipes.Length; i++)
        {
            CraftingRecipe recipe = sortedRecipes[i];
            if (lastRecipeType == null || lastRecipeType != recipe.recipeType)
            {
                MarginContainer category = craftingCategory.Instantiate<MarginContainer>();
                craftingListParent.AddChild(category);

                currentFlowContainer = category.FindChild("CraftingList") as FlowContainer;

                (category.FindChild("Label") as Label).Text = recipe.recipeType.ToString();
            }

            CraftingItemSlotComponent slot = inventorySlotScene.Instantiate<CraftingItemSlotComponent>();
            currentFlowContainer.AddChild(slot);
            inventoryButtons.Add(slot);

            SetSlot(i);

            int slotIndex = i;
            slot.MouseEntered += () =>
            {
                slotMouseOver = slotIndex;

                if (currentlySelected != slotIndex)
                {
                    slot.SetBackgroundStateHover();
                }
            };

            slot.MouseExited += () =>
            {
                slotMouseOver = -1;

                if (currentlySelected != slotIndex)
                {
                    slot.SetBackgroundStateDefault();
                }
            };

            lastRecipeType = recipe.recipeType;
        }

        SetSelectedCraftingSlot(0);
    }

    private void SetSlot(int slotIndex)
    {
        CraftingItemSlotComponent slot = inventoryButtons[slotIndex];
        CraftingRecipe recipe = sortedRecipes[slotIndex];
        slot.SetRecipe(recipe);
    }

    private void SetSelectedCraftingSlot(int slot)
    {
        if(slot == -1)
        {
            return;
        }

        if(currentlySelected >= 0)
        {
            inventoryButtons[currentlySelected].SetBackgroundStateDefault();
        }

        inventoryButtons[slot].SetBackgroundStateSelected();

        currentlySelected = slot;

        foreach (Node recipeComponent in recipeComponentContainer.GetChildren())
        {
            recipeComponent.Free();
        }

        CraftingRecipe recipe = sortedRecipes[slot];

        recipeTitleLabel.Text = recipe.result.item.displayName;
        recipeDescriptionLabel.Text = recipe.result.item.description;

        foreach (CraftingRecipeItem requiredItem in recipe.requiredItems)
        {
            InventoryItemDefinition item = requiredItem.item;

            HBoxContainer componentView = craftingComponentScene.Instantiate<HBoxContainer>();
            Label nameLabel = (componentView.FindChild("Name") as Label);
            nameLabel.Text = item.displayName;
            nameLabel.Modulate = item.rarity.GetTextColor();

            ItemSlotComponent slotData = componentView.FindChild("InventorySlot") as ItemSlotComponent;
            slotData.SetItem(item, requiredItem.count);

            bool hasItem = inventory.HasItem(item, requiredItem.count);
            componentView.Modulate = hasItem ? COLOR_WHITE : COLOR_TRANSPARENT;

            recipeComponentContainer.AddChild(componentView);
        }

        bool canCraftItem = inventory.CanCraftRecipe(recipe);
        craftButton.Disabled = !canCraftItem;
    }
}