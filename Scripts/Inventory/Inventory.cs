using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    private Dictionary<InventoryItem, int> itemInstanceLookup = new();

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

    public bool AddItem(InventoryItem item) => AddItem(item.definition, item.currentStackSize);

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
                    RefreshInventorySlot(existingIndex);
                    return true;
                }
                else
                {
                    count = (existingItem.currentStackSize + count) - item.stackSize;
                    existingItem.currentStackSize = item.stackSize;
                    RefreshInventorySlot(existingIndex);
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

        // Update lookups
        List<int> lookupList;
        if (!itemLookup.TryGetValue(item.itemType, out lookupList))
        {
            lookupList = new List<int>();
            itemLookup.Add(item.itemType, lookupList);
        }
        lookupList.Add(index);

        itemInstanceLookup.Add(inventoryItems[index], index);

        RefreshInventorySlot(index);
    }

    private void RefreshInventorySlot(int index)
    {
        if (quickSelectIndexes.Contains(index))
        {
            OnSetQuickslot.Invoke(index, quickSelectIndexes[index]);

            if (quickSelectEquipped == index)
            {
                OnSelectQuickslot(index);
            }
        }
    }

    public bool RemoveItem(InventoryItem item, int count)
    {
        int inventoryIndex = itemInstanceLookup[item];

        item.currentStackSize -= count;
        if(item.currentStackSize <= 0)
        {
            RemoveItem(inventoryIndex);
        }

        // Refresh slot
        RefreshInventorySlot(inventoryIndex);

        return true;
    }

    public bool RemoveItem(InventoryItemDefinition itemDefinition, int count)
    {
        if(!itemLookup.TryGetValue(itemDefinition.itemType, out var lookupList))
        {
            return false;
        }

        List<int> indexesToRemove = new();
        int finalIndex = -1;
        for (int i = 0; i < lookupList.Count; i++)
        {
            int inventoryIndex = lookupList[i];
            InventoryItem item = inventoryItems[inventoryIndex];
            if(item.currentStackSize <= count)
            {
                indexesToRemove.Add(inventoryIndex);
                count -= item.currentStackSize;
                continue;
            }

            finalIndex = inventoryIndex;
        }

        if(finalIndex == -1 && count != 0)
        {
            return false;
        }

        foreach(int indexToRemove in indexesToRemove)
        {
            RemoveItem(indexToRemove);
        }

        inventoryItems[finalIndex].currentStackSize -= count;

        return true;
    }

    public bool RemoveItem(int index)
    {
        var itemToRemove = inventoryItems[index];
        inventoryItems[index] = null;

        // Update lookups
        List<int> lookupList;
        if (itemLookup.TryGetValue(itemToRemove.definition.itemType, out lookupList))
        {
            lookupList.Remove(index);
        }

        itemInstanceLookup.Remove(itemToRemove);

        return true;
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