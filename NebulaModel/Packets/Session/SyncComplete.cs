#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI.GameState;
using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Session;

public class SyncComplete
{
    public SyncComplete()
    {
        AllPlayers = Array.Empty<PlayerData>();
        ClientCert = Array.Empty<byte>();
    }

    public SyncComplete(IEnumerable<IPlayerData> otherPlayers)
    {
        AllPlayers = otherPlayers.Select(data => (PlayerData)data).ToArray();
        ClientCert = Array.Empty<byte>();
    }

    public SyncComplete(byte[] clientCert)
    {
        AllPlayers = Array.Empty<PlayerData>();
        ClientCert = clientCert;
    }

    public PlayerData[] AllPlayers { get; set; }
    public byte[] ClientCert { get; set; }
}
