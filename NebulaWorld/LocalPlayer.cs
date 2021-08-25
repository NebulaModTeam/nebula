using NebulaModel.DataStructures;
using System;

namespace NebulaWorld
{
    public class LocalPlayer : IDisposable
    {
        public bool IsInitialDataReceived { get; private set; }
        public bool IsHost { get; set; }
        public bool IsClient => !IsHost;
        public ushort Id => Data.PlayerId;
        public PlayerData Data { get; private set; }

        public void SetPlayerData(PlayerData data)
        {
            Data = data;

            if (!IsInitialDataReceived)
            {
                IsInitialDataReceived = true;

                if (Multiplayer.Session.IsGameLoaded)
                {
                    Multiplayer.Session.World.SetupInitialPlayerState();
                }
            }
        }

        public void Dispose()
        {
            Data = null;
        }
    }
}
