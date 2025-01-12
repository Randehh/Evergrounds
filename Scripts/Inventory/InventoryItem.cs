public class InventoryItem
{
    public InventoryItemDefinition definition;
    public int currentStackSize = 1;

    public InventoryItem(InventoryItemDefinition definition, int currentStackSize = 1)
    {
        this.definition = definition;
        this.currentStackSize = currentStackSize;
    }
}