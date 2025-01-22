using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class WorldItemSpawner : Node2D
{
    private const string WORLD_ITEM_SCENE = @"res://Scenes/World/world_item.tscn";
    private const float SPAWN_INTERVAL = 0.075f;

    private PackedScene worldItemScene;
    private Queue<InventoryItemDefinition> toSpawn = new();
    private double spawningTime = 0;
    private bool isSpawning = false;

    public WorldItemSpawner(WorldItemSpawnerItemData[] itemsToSpawn)
    {
        SetItems(itemsToSpawn);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!isSpawning)
        {
            return;
        }

        spawningTime += delta;

        while (spawningTime > SPAWN_INTERVAL)
        {
            if (toSpawn.Count == 0)
            {
                isSpawning = false;
                QueueFree();
                return;
            }

            spawningTime -= SPAWN_INTERVAL;

            WorldItem worldItem = worldItemScene.Instantiate<WorldItem>();
            WorldMap.Instance.AddWorldNode(worldItem, true);
            worldItem.Initialize(toSpawn.Dequeue(), 1);
            worldItem.GlobalPosition = GlobalPosition;
            worldItem.Spawn();
        }
    }

    public void SetItems(WorldItemSpawnerItemData[] itemsToSpawn)
    {
        worldItemScene = GD.Load<PackedScene>(WORLD_ITEM_SCENE);
        spawningTime = SPAWN_INTERVAL;

        foreach (var itemData in itemsToSpawn)
        {
            itemData.InsertIntoQueue(toSpawn);
        }
        isSpawning = true;
    }
}