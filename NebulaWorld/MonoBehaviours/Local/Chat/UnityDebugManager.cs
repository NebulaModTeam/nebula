#region

using NebulaModel.Logger;
using UnityEngine;
using ILogger = NebulaModel.Logger.ILogger;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class UnityDebugManager : MonoBehaviour
{
    private void Awake()
    {
        VFInput.Init();
        Log.Init(new EditorLogger());
    }

    private void Update()
    {
        VFInput.OnUpdate();
    }
}

public class EditorLogger : ILogger
{
    public void LogDebug(object data)
    {
        Debug.Log(data);
    }

    public void LogInfo(object data)
    {
        Debug.Log(data);
    }

    public void LogWarning(object data)
    {
        Debug.LogWarning(data);
    }

    public void LogError(object data)
    {
        Debug.LogError(data);
    }
}
