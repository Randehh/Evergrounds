
using Godot;
using System.Collections.Generic;

public class TimeService : IService, IWorldSaveable
{
    private const string SAVE_KEY_DAY = "Day";

    public int currentDay = 1;

    private HashSet<IWorldTimeListener> worldTimeListeners = new();

    public void OnInit()
    {

    }

    public void OnReady()
    {
        ServiceLocator.GameNotificationService.OnNodeSpawned.OnFire += OnNodeSpawned;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire += OnNodeRemoved;
        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire += OnNodeRemoved;
    }

    public void OnDestroy()
    {
        ServiceLocator.GameNotificationService.OnNodeSpawned.OnFire -= OnNodeSpawned;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire -= OnNodeRemoved;
        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire -= OnNodeRemoved;
    }

    public void TriggerNextDay()
    {
        currentDay++;

        foreach (IWorldTimeListener timeListener in worldTimeListeners)
        {
            timeListener.SetTime(currentDay);
        }
    }

    private void OnNodeSpawned(WorldNodeData node)
    {
        if(node.LiveNode is IWorldTimeListener timeListener)
        {
            timeListener.SetTime(currentDay);
            worldTimeListeners.Add(timeListener);
        }
    }

    private void OnNodeRemoved(Node2D node)
    {
        if (node is IWorldTimeListener timeListener)
        {
            worldTimeListeners.Remove(timeListener);
        }
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        return new Godot.Collections.Dictionary<string, Variant>()
        {
            { SAVE_KEY_DAY, currentDay }
        };
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        currentDay = data[SAVE_KEY_DAY].AsInt32();
    }
}