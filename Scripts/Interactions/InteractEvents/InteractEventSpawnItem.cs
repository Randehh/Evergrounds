using Godot;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class InteractEventSpawnItem : InteractEvent
{

    private const string WORLD_ITEM_SCENE = @"res://Scenes/World/world_item.tscn";
    private const float SPAWN_INTERVAL = 0.075f;

    [Export]
    private Array<WorldItemSpawnerItemData> itemsToSpawn;

    private PackedScene worldItemScene;
    private bool isSpawning = false;
    private Queue<InventoryItemDefinition> toSpawn = new();
    private double spawningTime = 0;

    public override void _Ready()
    {
        worldItemScene = GD.Load<PackedScene>(WORLD_ITEM_SCENE);
    }

    public override void _Process(double delta)
    {
        if(!isSpawning)
        {
            return;
        }

        spawningTime += delta;

        while(spawningTime > SPAWN_INTERVAL)
        {
            if(toSpawn.Count == 0)
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

    public override void Execute()
    {
        Vector2 position = GlobalPosition;

        GetParent().RemoveChild(this);
        WorldMap.Instance.AddChild(this);

        GlobalPosition = position;

        toSpawn = new();
        foreach(var spawnItemData in itemsToSpawn)
        {
            spawnItemData.InsertIntoQueue(toSpawn);
        }

        isSpawning = true;
        spawningTime = SPAWN_INTERVAL;
    }
}