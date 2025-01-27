using Godot;
using System;
using System.Collections.Generic;

public class InventoryService : IService, IWorldSaveable
{
    private const string SAVE_KEY_QUICK_SELECT = "QuickSelect";
    private const string SAVE_KEY_ITEMS = "Items";
    private const string SAVE_KEY_ITEM_INDEX = "ItemIndex";
    private const string SAVE_KEY_ITEM_DEFINITION = "ItemDefinitionPath";
    private const string SAVE_KEY_ITEM_STACK_COUNT = "ItemStackCount";

    public Action<int> OnSelectQuickslot = delegate { };
    public Action<int> OnUpdateSlot = delegate { };

    public const int QUICK_SELECT_COUNT = 10;
    public const int INVENTORY_SIZE = 50;

    private InventoryItem[] inventoryItems = new InventoryItem[INVENTORY_SIZE];
    private int quickSelectEquipped = 0;

    private System.Collections.Generic.Dictionary<InventoryItemDefinition, List<int>> itemLookup = new();
    private System.Collections.Generic.Dictionary<InventoryItem, int> itemInstanceLookup = new();

    public void OnInit()
    {

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

    public void UpdateAllSlots()
    {
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            OnUpdateSlot.Invoke(i);
        }
    }

    public bool AddItem(InventoryItem item) => AddItem(item.definition, item.CurrentStackSize);

    public bool AddItem(InventoryItemDefinition item, int count)
    {
        // Try to stack first
        if (item.isStackable && itemLookup.TryGetValue(item, out List<int> existingIndexes))
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
        if (!itemLookup.TryGetValue(itemDefinition, out lookupList))
        {
            lookupList = new List<int>();
            itemLookup.Add(itemDefinition, lookupList);
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
        if (!itemLookup.TryGetValue(itemDefinition, out var lookupList))
        {
            return false;
        }

        List<int> indexesToRemove = new();
        int finalIndex = -1;
        for (int i = 0; i < lookupList.Count; i++)
        {
            int inventoryIndex = lookupList[i];
            InventoryItem item = inventoryItems[inventoryIndex];
            if (item.CurrentStackSize <= count)
            {
                indexesToRemove.Add(inventoryIndex);
                count -= item.CurrentStackSize;
                continue;
            }
            else
            {
                if (item.CurrentStackSize == count)
                {
                    RemoveItem(inventoryIndex);
                    return true;
                }
            }

            finalIndex = inventoryIndex;
        }

        if (finalIndex == -1 && count != 0)
        {
            return false;
        }

        foreach (int indexToRemove in indexesToRemove)
        {
            RemoveItem(indexToRemove);
        }

        if (finalIndex != -1)
        {
            inventoryItems[finalIndex].CurrentStackSize -= count;
        }

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
        if (itemLookup.TryGetValue(itemToRemove.definition, out lookupList))
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

    public void SwapItems(int index1, int index2)
    {
        InventoryItem cachedItem = inventoryItems[index2];
        inventoryItems[index2] = inventoryItems[index1];
        inventoryItems[index1] = cachedItem;
    }

    public bool HasItem(InventoryItemDefinition itemType, int count)
    {
        if(!itemLookup.TryGetValue(itemType, out List<int> itemIndexes))
        {
            return false;
        }

        int totalCount = 0;
        foreach (int itemIndex in itemIndexes)
        {
            totalCount += GetItem(itemIndex).CurrentStackSize;
        }

        return totalCount >= count;
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

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> data = new ();
        data.Add(SAVE_KEY_QUICK_SELECT, quickSelectEquipped);

        Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> itemsData = new();
        data.Add(SAVE_KEY_ITEMS, itemsData);
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            InventoryItem item = inventoryItems[i];
            if(item == null)
            {
                continue;
            }

            itemsData.Add(new Godot.Collections.Dictionary<string, Variant>
            {
                { SAVE_KEY_ITEM_INDEX, i },
                { SAVE_KEY_ITEM_DEFINITION, item.definition.ResourcePath },
                { SAVE_KEY_ITEM_STACK_COUNT, item.CurrentStackSize },
            });
        }

        return data;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        foreach(InventoryItem item in inventoryItems)
        {
            if(item == null)
            {
                continue;
            }

            item.onStackSizeUpdated -= OnStackSizeUpdated;
        }

        inventoryItems = new InventoryItem[inventoryItems.Length];
        itemLookup.Clear();
        itemInstanceLookup.Clear();

        foreach (Godot.Collections.Dictionary<string, Variant> itemData in data[SAVE_KEY_ITEMS].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
        {
            int index = itemData[SAVE_KEY_ITEM_INDEX].AsInt32();
            InventoryItemDefinition itemDefinition = GD.Load<InventoryItemDefinition>(itemData[SAVE_KEY_ITEM_DEFINITION].AsString());
            int stackCount = itemData[SAVE_KEY_ITEM_STACK_COUNT].AsInt32();

            SetItem(index, itemDefinition, stackCount);
        }

        UpdateAllSlots();

        EquipSlot(data[SAVE_KEY_QUICK_SELECT].AsInt32());
    }
}