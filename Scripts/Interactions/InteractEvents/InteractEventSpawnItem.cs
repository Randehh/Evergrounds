using Godot;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class InteractEventSpawnItem : InteractEvent
{

    private const string WORLD_ITEM_SCENE = @"res://Scenes/World/world_item.tscn";
    private const float SPAWN_INTERVAL = 0.1f;

    [Export]
    private Array<InventoryItemDefinition> itemsToSpawn;

    private PackedScene worldItemScene;
    private bool isSpawning = false;
    private Queue<InventoryItemDefinition> toSpawn = new();
    private double spawningTime = 0;

    public override void _Ready()
    {
        base._Ready();

        worldItemScene = GD.Load<PackedScene>(WORLD_ITEM_SCENE);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

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
                return;
            }

            spawningTime -= SPAWN_INTERVAL;

            WorldItem worldItem = worldItemScene.Instantiate<WorldItem>();
            GetParent().AddChild(worldItem);
            worldItem.Initialize(toSpawn.Dequeue(), 1);
            worldItem.GlobalPosition = GlobalPosition;
            worldItem.Spawn();
        }
    }

    public override void Execute()
    {
        toSpawn = new(itemsToSpawn);
        isSpawning = true;
    }
}