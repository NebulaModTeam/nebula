using System;

namespace NebulaAPI;

// Generic static classes will have an instance per generic type.
public static class SingletonCache<T> where T : class, new()
{
    private static T instance;

    public static T Create()
    {
        if (instance is not null)
            throw new InvalidOperationException($"A singleton instance of type {typeof(T).FullName} already exists.");

        instance = new T();
        return instance;
    }

    public static T GetOrCreate() => instance ??= new T();

    public static T Get() => instance;

    public static bool HasInstance() => instance is not null;

    public static void Destroy(T obj)
    {
        if (instance == obj)
            instance = null;
        else
            throw new ArgumentException("Object provided is not a cached singleton.");
    }
}
