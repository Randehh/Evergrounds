using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ServiceLocator : Node
{
    private static ServiceLocator Instance;

    public static InventoryService InventoryService => Instance.GetService<InventoryService>(true);
    public static GameNotificationService GameNotificationService => Instance.GetService<GameNotificationService>(true);
    public static TimeService TimeService => Instance.GetService<TimeService>(true);
    public static InputStateService InputStateService => Instance.GetService<InputStateService>(true);

    private Dictionary<Type, IService> services = new ();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        RegisterService(new TimeService());
        RegisterService(new InputStateService());
    }

    public override void _Process(double delta)
    {
        foreach (IService service in services.Values)
        {
            service.Process(delta);
        }
    }

    public T GetService<T>(bool createIfNotExists) where T : class, IService
    {
        if(!services.TryGetValue(typeof(T), out var service))
        {
            if (createIfNotExists)
            {
                service = Activator.CreateInstance<T>();
                GD.Print($"Creating service {typeof(T).Name}: {service}");
                RegisterService(service);
            }
            else
            {
                GD.PushError($"Tried getting service of type {typeof(T).Name}, but it was not registered");
                return default;
            }
        }

        return service as T;
    }

    public void RegisterService(IService service, bool replaceIfExists = false)
    {
        if (services.ContainsKey(service.GetType()))
        {
            if (replaceIfExists)
            {
                RemoveService(service);
            }
            else
            {
                GD.PushError($"Tried registering service of type {service.GetType().Name}, but it was already registered");
                return;
            }
        }

        services.Add(service.GetType(), service);
        service.OnInit();
        service.OnReady();
    }

    public void RemoveService<T>() where T : class, IService
    {
        if (!services.TryGetValue(typeof(T), out var service))
        {
            GD.PushError($"Tried removing service of type {typeof(T).Name}, but it was not registered");
            return;
        }

        RemoveService(service);
    }

    public void RemoveService(IService service)
    {
        services.Remove(service.GetType());
        service.OnDestroy();
    }
}
