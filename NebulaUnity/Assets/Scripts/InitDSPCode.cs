﻿using NebulaModel.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILogger = NebulaModel.Logger.ILogger;

public class InitDSPCode : MonoBehaviour
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