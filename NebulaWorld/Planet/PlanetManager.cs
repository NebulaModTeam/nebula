#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld.Planet;

public class PlanetManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public Dictionary<int, byte[]> PendingFactories { get; private set; } = new();
    public Dictionary<int, byte[]> PendingTerrainData { get; private set; } = new();
    public bool EnableVeinPacket { get; set; } = true;

    public void Dispose()
    {
        PendingFactories = null;
        PendingTerrainData = null;
        GC.SuppressFinalize(this);
    }
}
