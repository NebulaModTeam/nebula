#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace NebulaModel.Utils;

public class UnityDispatchQueue : MonoBehaviour
{
    private static UnityDispatchQueue _instance;

    private readonly Queue<Action> actionsQueue = new();

    private void Update()
    {
        lock (actionsQueue)
        {
            while (actionsQueue.Count > 0)
            {
                actionsQueue.Dequeue().Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private static UnityDispatchQueue GetInstance()
    {
        if (!_instance)
        {
            _instance = FindObjectOfType<UnityDispatchQueue>();
        }

        if (_instance)
        {
            return _instance;
        }
        var go = new GameObject(nameof(UnityDispatchQueue));
        _instance = go.AddComponent<UnityDispatchQueue>();
        DontDestroyOnLoad(_instance);

        return _instance;
    }

    public static void RunOnMainThread(Action action)
    {
        var instance = GetInstance();
        lock (instance.actionsQueue)
        {
            instance.actionsQueue.Enqueue(action);
        }
    }
}
