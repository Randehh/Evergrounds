using Godot;
using System.Collections.Generic;

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
    private Control expandParent;

    [Export]
    private FlowContainer slotContainer;

    [Export]
    private Texture2D slotBackground;

    [Export]
    private Texture2D slotBackgroundHover;

    [Export]
    private Texture2D slotBackgroundSelected;

    [Export]
    private Label recipeTitleLabel;

    [Export]
    private Label recipeDescriptionLabel;

    [Export]
    private VBoxContainer recipeComponentContainer;

    [Export]
    private Button craftButton;

    [Export]
    private CraftingRecipeContainer defaultCraftingRecipeContainer;

    private InventoryService inventory;
    private CraftingRecipeContainer recipeContainer;
    private int slotMouseOver = -1;
    private List<CraftingItemSlotComponent> inventoryButtons = new();
    private int currentlySelected = -1;

    private float foldedY;
    private float expandedY;
    private bool isExpanded = false;

    public override void _Ready()
    {
        inventory = ServiceLocator.InventoryService;
        recipeContainer = defaultCraftingRecipeContainer;

        craftButton.Pressed += CraftButton_Pressed;

        ReloadUI();

        // We can be smarter here and track which indexes are being used in components, but we're cavemanning it until it stops working
        inventory.OnUpdateSlot += (_) =>
        {
            SetSelectedCraftingSlot(currentlySelected);
        };
    }   

    private void CraftButton_Pressed()
    {
        CraftingRecipe recipe = defaultCraftingRecipeContainer.recipes[currentlySelected];
        foreach (var item in recipe.requiredItems)
        {
            inventory.RemoveItem(item.item, item.count);
        }

        inventory.AddItem(recipe.result.item, recipe.result.count);
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

        if(Input.IsActionJustPressed("inventory_toggle"))
        {
            isExpanded = !isExpanded;
        }

        expandParent.Position = new Vector2(expandParent.Position.X, (float)Mathf.Lerp(expandParent.Position.Y, isExpanded ? expandedY : foldedY, 20 * delta));
    }

    private void ReloadUI()
    {
        inventoryButtons.Clear();

        foreach(var child in slotContainer.GetChildren())
        {
            slotContainer.RemoveChild(child);
            child.Free();
        }

        for (int i = 0; i < defaultCraftingRecipeContainer.recipes.Count; i++)
        {
            CraftingRecipe recipe = defaultCraftingRecipeContainer.recipes[i];
            CraftingItemSlotComponent slot = inventorySlotScene.Instantiate<CraftingItemSlotComponent>();
            slotContainer.AddChild(slot);
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
        }

        foldedY = -310;
        expandedY = 66;

        SetSelectedCraftingSlot(0);
    }

    private void SetSlot(int slotIndex)
    {
        CraftingItemSlotComponent slot = inventoryButtons[slotIndex];
        CraftingRecipe recipe = defaultCraftingRecipeContainer.recipes[slotIndex];
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

        CraftingRecipe recipe = recipeContainer.recipes[slot];

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