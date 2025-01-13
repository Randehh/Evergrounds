using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class WorldItemSpawnerItemData : Resource
{

    [Export]
    public InventoryItemDefinition item;

    [Export]
    private int minimumCount = 1;

    [Export]
    private int maximumCount = 1;

    [Export]
    private float baseChance = 1;

    public WorldItemSpawnerItemData() { }

    public WorldItemSpawnerItemData(InventoryItemDefinition item, int minimumCount, int maximumCount, float baseChance)
    {
        this.item = item;
        this.minimumCount = minimumCount;
        this.maximumCount = maximumCount;
        this.baseChance = baseChance;
    }

    public void InsertIntoQueue(Queue<InventoryItemDefinition> queue)
    {
        float chanceRoll = GD.Randf();
        if(chanceRoll > baseChance)
        {
            return;
        }

        int count = GD.RandRange(minimumCount, maximumCount);
        for (int i = 0; i < count; i++)
        {
            queue.Enqueue(item);
        }
    }
}