using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Inventory : Node
{
    public static Inventory Instance;

    public Action<int> OnSelectQuickslot = delegate { };
    public Action<int, int> OnSetQuickslot = delegate { };

    public const int QUICK_SELECT_COUNT = 10;
    public const int INVENTORY_SIZE = 50;

    [Export]
    private InventoryItemDefinition[] itemsToAdd;

    private InventoryItem[] inventoryItems = new InventoryItem[INVENTORY_SIZE];
    private int quickSelectEquipped = 0;
    private int[] quickSelectIndexes = new int[QUICK_SELECT_COUNT];

    private Dictionary<InventoryItemType, List<int>> itemLookup = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        for (int i = 0; i < QUICK_SELECT_COUNT; i++)
        {
            quickSelectIndexes[i] = i;
        }

        foreach (var item in itemsToAdd)
        {
            AddItem(item, item.stackSize);
        }

        EquipSlot(0);
    }

    public override void _Process(double delta)
    {

        for (int i = 0; i < QUICK_SELECT_COUNT; i++)
        {
            if (Input.IsActionJustPressed($"equip_{i}"))
            {
                int buttonIndex = i - 1;
                if (buttonIndex == -1)
                {
                    buttonIndex = 9;
                }
                EquipSlot(buttonIndex);
            }
        }

        if (Input.IsActionJustPressed("equip_next"))
        {
            EquipSlot(quickSelectEquipped + 1);
        }

        if (Input.IsActionJustPressed("equip_previous"))
        {
            EquipSlot(quickSelectEquipped - 1);
        }
    }

    public void EmitQuickslotCallbacks()
    {
        for (int i = 0; i < quickSelectIndexes.Length; i++)
        {
            int inventoryIndex = quickSelectIndexes[i];
            OnSetQuickslot.Invoke(i, inventoryIndex);
        }
    }

    public void AddItem(InventoryItem item) => AddItem(item.definition, item.currentStackSize);

    public bool AddItem(InventoryItemDefinition item, int count)
    {
        // Try to stack first
        if (item.isStackable && itemLookup.TryGetValue(item.itemType, out List<int> existingIndexes))
        {
            foreach (int existingIndex in existingIndexes)
            {
                InventoryItem existingItem = inventoryItems[existingIndex];
                if (existingItem.currentStackSize + count <= item.stackSize)
                {
                    existingItem.currentStackSize += count;
                    return true;
                }
                else
                {
                    count = item.stackSize - (existingItem.currentStackSize + count);
                    existingItem.currentStackSize = item.stackSize;
                }
            }
        }

        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] == null)
            {
                SetItem(i, item, count);
                return true;
            }
        }

        return false;
    }


    public void SetItem(int index, InventoryItemDefinition item, int count)
    {
        inventoryItems[index] = new InventoryItem(item, count);

        if(quickSelectIndexes.Contains(index))
        {
            OnSetQuickslot.Invoke(index, quickSelectIndexes[index]);

            if(quickSelectEquipped == index)
            {
                OnSelectQuickslot(index);
            }
        }
    }

    public void RemoveItem(InventoryItemDefinition item)
    {

    }

    public void RemoveItem(int index)
    {
        inventoryItems[index] = null;
    }

    public InventoryItem GetItem(int index)
    {
        return inventoryItems[index];
    }

    public InventoryItem GetItemFromQuickslot(int index)
    {
        return GetItem(quickSelectIndexes[index]);
    }

    public void SwapItems(int index1, int index2)
    {
        InventoryItem cachedItem = inventoryItems[index2];
        inventoryItems[index2] = inventoryItems[index1];
        inventoryItems[index1] = cachedItem;
    }

    private void EquipSlot(int slot)
    {
        if (slot < 0)
        {
            slot = QUICK_SELECT_COUNT - 1;
        }
        else if (slot > QUICK_SELECT_COUNT - 1)
        {
            slot = 0;
        }

        quickSelectEquipped = slot;

        int nextButtonIndex = slot;
        OnSelectQuickslot.Invoke(nextButtonIndex);
    }
}