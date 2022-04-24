using NebulaModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NebulaWorld.Planet
{
    public class PlanetManager : IDisposable
    {
        public Dictionary<int, byte[]> PendingFactories { get; private set; }
        public Dictionary<int, byte[]> PendingTerrainData { get; private set; }

        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();

        public PlanetManager()
        {
            PendingFactories = new Dictionary<int, byte[]>();
            PendingTerrainData = new Dictionary<int, byte[]>();
        }

        public void Dispose()
        {
            PendingFactories = null;
            PendingTerrainData = null;
        }
    }
}
