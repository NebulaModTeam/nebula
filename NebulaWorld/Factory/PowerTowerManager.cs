#region

using System;
using System.Collections.Generic;
using NebulaModel.Packets.Factory.PowerTower;

#endregion

namespace NebulaWorld.Factory;

public class PowerTowerManager : IDisposable
{
    public HashSet<int> LocalChargerIds = [];
    public HashSet<long> RemoteChargerHashIds = [];

    public void Dispose()
    {
        LocalChargerIds.Clear();
        LocalChargerIds = null;

        RemoteChargerHashIds.Clear();
        RemoteChargerHashIds = null;

        GC.SuppressFinalize(this);
    }

    public void OnClientDisconnect()
    {
        // Procast event to reset all
        LocalChargerIds.Clear();
        RemoteChargerHashIds.Clear();
        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerChargerUpdate(
            -1,
            0,
            false));
    }
}
