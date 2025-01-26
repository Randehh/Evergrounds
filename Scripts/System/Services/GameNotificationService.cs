
using Godot;
using System;

public class GameNotificationService : IService
{
    public GameNotification<WorldData> OnLoadWorld = new();
    public GameNotification<Node2D> OnNodeDespawned = new();
    public GameNotification<Node2D> OnNodeDestroyed = new();

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

public class GameNotification<T> where T : class
{
    public void Execute(T payload)
    {
        OnPreFire(payload);
        OnFire(payload);
    }

    public Action<T> OnPreFire {  get; set; } = delegate { };
    public Action<T> OnFire { get; set; } = delegate { };
}