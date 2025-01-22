using Godot;
using System;
using System.Collections.Generic;

public class ServiceLocator
{
    private static ServiceLocator Instance = new();

    public static InventoryService InventoryService => Instance.GetService<InventoryService>(true);
    public static GameNotificationService GameNotificationService => Instance.GetService<GameNotificationService>(true);

    private Dictionary<Type, IService> services = new ();

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
