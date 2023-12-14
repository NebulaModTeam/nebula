#region

using System.Linq;
using NebulaAPI;
using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Session;

public class SyncComplete
{
    public SyncComplete()
    {
        AllPlayers = new PlayerData[] { };
        ClientCert = new byte[] { };
    }

    public SyncComplete(IPlayerData[] otherPlayers)
    {
        AllPlayers = otherPlayers.Select(data => (PlayerData)data).ToArray();
        ClientCert = new byte[] { };
    }

    public SyncComplete(byte[] clientCert)
    {
        AllPlayers = new PlayerData[] { };
        ClientCert = clientCert;
    }

    public PlayerData[] AllPlayers { get; set; }
    public byte[] ClientCert { get; set; }
}
