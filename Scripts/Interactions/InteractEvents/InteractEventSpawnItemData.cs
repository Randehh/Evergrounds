using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InteractEventSpawnItemData : Resource
{

    [Export]
    public InventoryItemDefinition item;

    [Export]
    private int minimumCount = 1;

    [Export]
    private int maximumCount = 1;

    [Export]
    private float baseChance = 1;

    public InteractEventSpawnItemData() { }

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