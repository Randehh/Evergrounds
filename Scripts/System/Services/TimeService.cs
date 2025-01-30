
using Godot;
using System;
using System.Collections.Generic;

public class TimeService : IService, IWorldSaveable
{
    private const string SAVE_KEY_DAY = "Day";
    private const string SAVE_KEY_TIME = "Time";

    // 10 minutes = 1 day
    private const float TIME_MULTIPLIER = 24 * (60 / 10);
    private const int SECONDS_IN_DAY = 86400;
    private const int DAY_START_TIME = (SECONDS_IN_DAY / 24) * 6;

    public int currentDay = 1;

    private double currentTime = 0;

    private HashSet<IWorldTimeListener> worldTimeListeners = new();

    public void OnInit()
    {
        currentTime = DAY_START_TIME;
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

    public void Process(double delta)
    {
        currentTime += delta * TIME_MULTIPLIER;

        if(currentTime >= SECONDS_IN_DAY)
        {
            TriggerNextDay(false);
            currentTime -= SECONDS_IN_DAY;
        }

        ServiceLocator.GameNotificationService.OnTimeUpdated.Execute(new TimeUpdatePayload()
        {
            day = currentDay,
            time = currentTime,
            normalizedTime = SECONDS_IN_DAY / currentTime,
            displayString = new TimeSpan(0, 0, 0, (int)currentTime, 0).ToString("hh\\:mm")
        });
    }

    public void TriggerNextDay(bool useTransition)
    {
        if(useTransition)
        {
            ServiceLocator.GameNotificationService.OnTriggerDayTransition.Execute(() => TriggerNextDay(false));
            return;
        }

        currentDay++;
        currentTime = DAY_START_TIME;

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
            { SAVE_KEY_DAY, currentDay },
            { SAVE_KEY_TIME, currentTime },
        };
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        currentDay = data[SAVE_KEY_DAY].AsInt32();
        currentTime = (float)data[SAVE_KEY_TIME];
    }
}

public struct TimeUpdatePayload
{
    public int day;
    public double time;
    public double normalizedTime;
    public string displayString;
}