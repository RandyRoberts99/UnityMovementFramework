using Radknee.Services;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceManager
{
    private static readonly HashSet<IService> _services = new HashSet<IService>();

    public static void Process()
    {
        foreach (var service in _services)
        {
            service.Process();
        }
    }

    public static bool RegisterService<T>(T service) where T : IService
    {
        try
        {
            _services.Add(service);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to register service: {ex.Message}");
            return false;
        }

        return true;
    }

    public static T GetService<T>() where T : IService
    {
        foreach (var service in _services)
        {
            if (service is T typedService)
            {
                return typedService;
            }
        }

        return default;
    }
}
