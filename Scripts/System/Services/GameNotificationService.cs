
using Godot;
using System;

public class GameNotificationService : IService
{
    // Saving and loading
    public GameNotification<WorldData> OnLoadWorld = new();

    // Nodes
    public GameNotification<WorldNodeData> OnNodeSpawned = new();
    public GameNotification<Node2D> OnNodeDespawned = new();
    public GameNotification<Node2D> OnNodeDestroyed = new();

    // Time
    public GameNotification<int> OnNextDay = new();

    public void OnInit()
    {

    }

    public void OnReady()
    {

    }

    public void OnDestroy()
    {

    }
}

public class GameNotification<T>
{
    public void Execute(T payload)
    {
        OnPreFire(payload);
        OnFire(payload);
    }

    public Action<T> OnPreFire {  get; set; } = delegate { };
    public Action<T> OnFire { get; set; } = delegate { };
}