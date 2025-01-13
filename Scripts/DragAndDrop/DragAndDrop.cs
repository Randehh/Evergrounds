using Godot;
using System;

[GlobalClass]
public partial class DragAndDrop : Node
{

    public static DragAndDrop Instance { get; private set; }

    public InventoryItem DraggingItem { get; private set; }
    public Action<InventoryItem> OnDraggingItemChanged { get; set; } = delegate { };

    public override void _Ready()
    {
        base._Ready();

        Instance = this;
    }

    public void StartDragging(InventoryItem item)
    {
        DraggingItem = item;
        OnDraggingItemChanged(item);
    }

    public void StopDragging()
    {
        DraggingItem = null;
        OnDraggingItemChanged(null);
    }
}