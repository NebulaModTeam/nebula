using NebulaModel.DataStructures;
using System;

namespace NebulaWorld
{
    public class LocalPlayer : IDisposable
    {
        public bool IsHost { get; set; }
        public bool IsClient => !IsHost;
        public ushort Id => Data.PlayerId;
        public PlayerData Data { get; private set; }

        public void SetPlayerData(PlayerData data)
        {
            Data = data;
        }

        public void Dispose()
        {
            Data = null;
        }
    }
}
