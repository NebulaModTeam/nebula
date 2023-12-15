#region

using System;
using System.Collections;
using System.Threading;

#endregion

namespace NebulaAPI.DataStructures;

public static class CollectionExtensions
{
    public static Locker Lock(this ICollection collection)
    {
        return new Locker(collection.SyncRoot);
    }

    public static Locker GetLocked<T>(this T collection, out T result) where T : ICollection
    {
        result = collection;
        return new Locker(collection.SyncRoot);
    }
}

public readonly struct Locker : IDisposable
{
    private readonly object lockObject;

    public Locker(object lockObject)
    {
        this.lockObject = lockObject;

        Monitor.Enter(lockObject);
    }

    public void Dispose()
    {
        Monitor.Exit(lockObject);
    }
}
