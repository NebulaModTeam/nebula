using NebulaModel.DataStructures;
using System;

namespace NebulaWorld.GameDataHistory
{
    public class GameDataHistoryManager : IDisposable
    {
        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();

        public GameDataHistoryManager()
        {
        }

        public void Dispose()
        {
        }
    }
}
