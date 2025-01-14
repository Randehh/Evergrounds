using System;

public class InventoryItem
{
    public Action<InventoryItem> onStackSizeUpdated = delegate { };

    public InventoryItemDefinition definition;

    private int currentStackSize = 1;
    public int CurrentStackSize {
        get => currentStackSize;
        set {
            currentStackSize = value;
            onStackSizeUpdated(this);
        }
    }

    public InventoryItem(InventoryItemDefinition definition, int currentStackSize = 1)
    {
        this.definition = definition;
        this.CurrentStackSize = currentStackSize;
    }
}