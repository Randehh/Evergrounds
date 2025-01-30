using Godot;

[GlobalClass]
public partial class ItemSlotComponent : TextureRect
{
    [Export]
    public TextureRect backgroundRect;

    [Export]
    public TextureRect textureRect;

    [Export]
    public Label quickSelectLabel;

    [Export]
    public Label stackCountLabel;

    [Export]
    private Texture2D slotBackground;

    [Export]
    private Texture2D slotBackgroundHover;

    [Export]
    private Texture2D slotBackgroundSelected;

    public override void _Ready()
    {
        base._Ready();

        quickSelectLabel.Visible = false;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void SetItem(InventoryItem item)
    {
        SetItem(item?.definition, item?.CurrentStackSize ?? 1);
    }

    public void SetItem(InventoryItemDefinition itemDefinition, int stackSize)
    {
        InventoryItemRarity rarity = itemDefinition != null ? itemDefinition.rarity : InventoryItemRarity.COMMON;

        backgroundRect.SelfModulate = rarity.GetSlotColor();
        textureRect.Texture = itemDefinition?.itemSprite;

        stackCountLabel.Text = stackSize.ToString();
        stackCountLabel.Visible = itemDefinition?.isStackable ?? false;
    }

    public void SetBackgroundStateDefault() => SetBackgroundTexture(slotBackground);
    public void SetBackgroundStateHover() => SetBackgroundTexture(slotBackgroundHover);
    public void SetBackgroundStateSelected() => SetBackgroundTexture(slotBackgroundSelected);

    private void SetBackgroundTexture(Texture2D texture)
    {
        backgroundRect.Texture = texture;
    }
}