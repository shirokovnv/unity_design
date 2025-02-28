using System;
using System.Collections.Generic;

public class ServiceLocator
{
    private readonly Dictionary<Type, IService> services;

    public bool Register<T>(T service) where T : IService
    {
        var key = service.GetType();
        if (services.ContainsKey(key))
        {
            return false;
        }

        services[key] = service;
        return true;
    }

    public bool Unregister<T>() where T : IService
    {
        var key = typeof(T);
        if (!services.ContainsKey(key))
        {
            return false;
        }

        return services.Remove(key);
    }

    public T Get<T>() where T : IService
    {
        var key = typeof(T);

        if (!services.ContainsKey(key))
        {
            throw new Exception($"Service of type {key.Name} not registered.");
        }

        return (T)services[key];
    }

    public bool TryGet<T>(out T service) where T : IService
    {
        var key = typeof(T);

        if (services.TryGetValue(key, out IService resolved))
        {
            service = (T)resolved;

            return true;
        }

        service = default;

        return false;
    }
}
