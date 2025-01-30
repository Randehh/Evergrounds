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
    private List<ButtonData> inventoryButtons = new();
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
            ReloadUI();
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
            TextureRect slot = inventorySlotScene.Instantiate<TextureRect>();
            slotContainer.AddChild(slot);

            ButtonData buttonData = new ButtonData(slot);
            inventoryButtons.Add(buttonData);

            SetSlot(i);

            int slotIndex = i;
            slot.MouseEntered += () =>
            {
                slotMouseOver = slotIndex;

                if (slot.Texture != slotBackgroundSelected)
                {
                    slot.Texture = slotBackgroundHover;
                }
            };

            slot.MouseExited += () =>
            {
                slotMouseOver = -1;

                if (slot.Texture != slotBackgroundSelected)
                {
                    slot.Texture = slotBackground;
                }
            };

            bool canCraftItem = CanCraftRecipe(recipe);
            slot.SelfModulate = recipe.result.item.rarity.GetSlotColor().WithAlpha(canCraftItem ? 1 : 0.5f);
        }

        foldedY = -310;
        expandedY = 66;

        SetSelectedCraftingSlot(0);
    }

    private void SetSlot(int slot)
    {
        ButtonData buttonData = inventoryButtons[slot];
        CraftingRecipe recipe = defaultCraftingRecipeContainer.recipes[slot];
        buttonData.label.Visible = false;
        buttonData.textureRect.Texture = recipe.result.item.itemSprite;
        buttonData.stackCountLabel.Text = recipe.result.count.ToString();
        buttonData.stackCountLabel.Visible = recipe.result.count > 1;
    }

    private void SetSelectedCraftingSlot(int slot)
    {
        if(slot == -1)
        {
            return;
        }

        if(currentlySelected >= 0)
        {
            inventoryButtons[currentlySelected].buttonRect.Texture = slotBackground;
        }

        inventoryButtons[slot].buttonRect.Texture = slotBackgroundSelected;

        currentlySelected = slot;

        foreach (Node recipeComponent in recipeComponentContainer.GetChildren())
        {
            recipeComponent.Free();
        }

        CraftingRecipe recipe = recipeContainer.recipes[slot];
        bool canCraftItem = CanCraftRecipe(recipe);

        recipeTitleLabel.Text = recipe.result.item.displayName;
        recipeDescriptionLabel.Text = recipe.result.item.description;

        foreach (CraftingRecipeItem requiredItem in recipeContainer.recipes[slot].requiredItems)
        {
            InventoryItemDefinition item = requiredItem.item;

            HBoxContainer componentView = craftingComponentScene.Instantiate<HBoxContainer>();
            Label nameLabel = (componentView.FindChild("Name") as Label);
            nameLabel.Text = item.displayName;
            nameLabel.Modulate = item.rarity.GetTextColor();

            ButtonData slotData = new ButtonData(componentView.FindChild("InventorySlot") as TextureRect);
            slotData.label.Visible = false;
            slotData.buttonRect.SelfModulate = item.rarity.GetSlotColor();
            slotData.textureRect.Texture = item.itemSprite;
            slotData.stackCountLabel.Text = requiredItem.count.ToString();
            slotData.stackCountLabel.Visible = requiredItem.count > 1;

            bool hasItem = inventory.HasItem(item, requiredItem.count);
            componentView.Modulate = hasItem ? COLOR_WHITE : COLOR_TRANSPARENT;

            recipeComponentContainer.AddChild(componentView);
        }

        craftButton.Disabled = !canCraftItem;
    }

    private bool CanCraftRecipe(CraftingRecipe recipe)
    {
        bool canCraftItem = true;

        foreach (CraftingRecipeItem requiredItem in recipe.requiredItems)
        {
            InventoryItemDefinition item = requiredItem.item;

            bool hasItem = inventory.HasItem(item, requiredItem.count);
            if (!hasItem)
            {
                canCraftItem = false;
            }
        }

        return canCraftItem;
    }

    private struct ButtonData
    {
        public TextureRect buttonRect;
        public TextureRect textureRect;
        public Label label;
        public Label stackCountLabel;

        public ButtonData(TextureRect buttonRect)
        {
            this.buttonRect = buttonRect;
            textureRect = buttonRect.GetNode("ItemSprite") as TextureRect;
            label = buttonRect.GetNode("NumberLabel") as Label;
            stackCountLabel = buttonRect.GetNode("StackCountLabel") as Label;
        }
    }
}