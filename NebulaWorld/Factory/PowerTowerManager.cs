#region

using System;
using System.Collections.Generic;
using NebulaModel.Packets.Factory.PowerTower;
#pragma warning disable IDE1006

#endregion

namespace NebulaWorld.Factory;

public class PowerTowerManager : IDisposable
{
    public HashSet<int> LocalChargerIds = []; // nodeId
    public Dictionary<long, int> RemoteChargerHashIds = []; // (plaentId << 32 | nodeId), playerCount

    public void Dispose()
    {
        LocalChargerIds.Clear();
        LocalChargerIds = null;

        RemoteChargerHashIds.Clear();
        RemoteChargerHashIds = null;

        GC.SuppressFinalize(this);
    }

    public void ResetAndBroadcast()
    {
        // Procast event to reset all
        LocalChargerIds.Clear();
        RemoteChargerHashIds.Clear();
        Multiplayer.Session.Network.SendPacket(new PowerTowerChargerUpdate(
            -1,
            0,
            false));
    }
}
