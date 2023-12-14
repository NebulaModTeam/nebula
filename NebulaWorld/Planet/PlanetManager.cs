#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld.Planet;

public class PlanetManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public PlanetManager()
    {
        PendingFactories = new Dictionary<int, byte[]>();
        PendingTerrainData = new Dictionary<int, byte[]>();
        EnableVeinPacket = true;
    }

    public Dictionary<int, byte[]> PendingFactories { get; private set; }
    public Dictionary<int, byte[]> PendingTerrainData { get; private set; }
    public bool EnableVeinPacket { get; set; } = true;

    public void Dispose()
    {
        PendingFactories = null;
        PendingTerrainData = null;
    }
}
