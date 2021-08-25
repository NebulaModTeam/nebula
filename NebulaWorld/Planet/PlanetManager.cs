using NebulaModel.DataStructures;
using System;

namespace NebulaWorld.Planet
{
    public class PlanetManager : IDisposable
    {
        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();

        public PlanetManager()
        {
        }

        public void Dispose()
        {
        }
    }
}
