using Godot;
using System;
using System.Collections.Generic;

public class InventoryService : IService
{

    public Action<int> OnSelectQuickslot = delegate { };
    public Action<int> OnUpdateSlot = delegate { };

    public const int QUICK_SELECT_COUNT = 10;
    public const int INVENTORY_SIZE = 50;

    private InventoryItem[] inventoryItems = new InventoryItem[INVENTORY_SIZE];
    private int quickSelectEquipped = 0;
    private int[] quickSelectIndexes = new int[QUICK_SELECT_COUNT];

    private Dictionary<InventoryItemType, List<int>> itemLookup = new();
    private Dictionary<InventoryItem, int> itemInstanceLookup = new();

    public void OnInit()
    {
        for (int i = 0; i < QUICK_SELECT_COUNT; i++)
        {
            quickSelectIndexes[i] = i;
        }
    }

    public void OnReady()
    {
        EquipSlot(0);
    }

    public void OnDestroy()
    {
        OnSelectQuickslot = null;
        OnUpdateSlot = null;
    }

    public void EmitQuickslotCallbacks()
    {
        for (int i = 0; i < quickSelectIndexes.Length; i++)
        {
            int inventoryIndex = quickSelectIndexes[i];
            OnUpdateSlot.Invoke(i);
        }
    }

    public bool AddItem(InventoryItem item) => AddItem(item.definition, item.CurrentStackSize);

    public bool AddItem(InventoryItemDefinition item, int count)
    {
        // Try to stack first
        if (item.isStackable && itemLookup.TryGetValue(item.itemType, out List<int> existingIndexes))
        {
            foreach (int existingIndex in existingIndexes)
            {
                InventoryItem existingItem = inventoryItems[existingIndex];
                if (existingItem.CurrentStackSize + count <= item.stackSize)
                {
                    existingItem.CurrentStackSize += count;
                    return true;
                }
                else
                {
                    count = (existingItem.CurrentStackSize + count) - item.stackSize;
                    existingItem.CurrentStackSize = item.stackSize;
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

    public void SetItem(int index, InventoryItemDefinition itemDefinition, int count)
    {
        InventoryItem item = new InventoryItem(itemDefinition, count);
        inventoryItems[index] = item;

        // Update lookups
        List<int> lookupList;
        if (!itemLookup.TryGetValue(itemDefinition.itemType, out lookupList))
        {
            lookupList = new List<int>();
            itemLookup.Add(itemDefinition.itemType, lookupList);
        }
        lookupList.Add(index);

        itemInstanceLookup.Add(item, index);

        item.onStackSizeUpdated += OnStackSizeUpdated;

        RefreshInventorySlot(index);
    }

    private void OnStackSizeUpdated(InventoryItem item)
    {
        RefreshInventorySlot(itemInstanceLookup[item]);
    }

    private void RefreshInventorySlot(int index)
    {
        OnUpdateSlot.Invoke(index);

        if (quickSelectEquipped == index)
        {
            OnSelectQuickslot(index);
        }
    }

    public bool RemoveItem(InventoryItem item, int count)
    {
        int inventoryIndex = itemInstanceLookup[item];

        item.CurrentStackSize -= count;
        if(item.CurrentStackSize <= 0)
        {
            RemoveItem(inventoryIndex);
        }

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
            if(item.CurrentStackSize <= count)
            {
                indexesToRemove.Add(inventoryIndex);
                count -= item.CurrentStackSize;
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

        inventoryItems[finalIndex].CurrentStackSize -= count;

        return true;
    }

    public bool RemoveItem(int index)
    {
        var itemToRemove = inventoryItems[index];

        if(itemToRemove == null)
        {
            return false;
        }

        inventoryItems[index] = null;

        // Update lookups
        List<int> lookupList;
        if (itemLookup.TryGetValue(itemToRemove.definition.itemType, out lookupList))
        {
            lookupList.Remove(index);
        }

        itemInstanceLookup.Remove(itemToRemove);

        itemToRemove.onStackSizeUpdated -= OnStackSizeUpdated;

        RefreshInventorySlot(index);

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

    public void EquipSlotNext() => EquipSlot(quickSelectEquipped + 1);
    public void EquipSlotPrevious() => EquipSlot(quickSelectEquipped - 1);

    public void EquipSlot(int slot)
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