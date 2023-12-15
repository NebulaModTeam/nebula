#region

using System;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld.GameDataHistory;

public class GameDataHistoryManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();


    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
