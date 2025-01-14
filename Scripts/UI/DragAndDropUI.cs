using Godot;

[GlobalClass]
public partial class DragAndDropUI : Control
{
    [Export]
    private Control mouseParent;

    [Export]
    private TextureRect itemDisplay;

    [Export]
    private Label itemCountLabel;

    public override void _Ready()
    {
        DragAndDrop.Instance.OnDraggingItemChanged += OnDraggingItemChanged;

        mouseParent.Visible = false;
    }

    public override void _Process(double delta)
    {
        Vector2 mousePosition = GetLocalMousePosition();
        mouseParent.Position = mouseParent.Position.Lerp(mousePosition, (20 + 1 / mousePosition.DistanceTo(mouseParent.Position)) * (float)delta);
    }

    private void OnDraggingItemChanged(InventoryItem item)
    {
        if (item == null)
        {
            mouseParent.Visible = false;
        }
        else
        {
            mouseParent.Visible = true;
            itemDisplay.Texture = item.definition.itemSprite;
            itemCountLabel.Visible = item.definition.isStackable;
            itemCountLabel.Text = item.CurrentStackSize.ToString();
        }
    }
}