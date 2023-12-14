#region

using System;
using NebulaAPI;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld;

public class LocalPlayer : IDisposable, ILocalPlayer
{
    public void Dispose()
    {
        Data = null;
    }

    public bool IsInitialDataReceived { get; private set; }
    public bool IsHost { get; set; }
    public bool IsClient => !IsHost;
    public bool IsNewPlayer { get; private set; }
    public ushort Id => Data.PlayerId;
    public IPlayerData Data { get; private set; }

    public void SetPlayerData(PlayerData data, bool isNewPlayer)
    {
        Data = data;
        IsNewPlayer = isNewPlayer;

        if (!IsInitialDataReceived)
        {
            IsInitialDataReceived = true;

            if (Multiplayer.Session.IsGameLoaded)
            {
                Multiplayer.Session.World.SetupInitialPlayerState();
            }
        }
    }
}
