using NebulaAPI;
using NebulaModel.DataStructures;
using System.Linq;

namespace NebulaModel.Packets.Session
{
    public class SyncComplete
    {
        public PlayerData[] AllPlayers { get; set; }

        public SyncComplete() { AllPlayers = new PlayerData[] { }; }
        public SyncComplete(IPlayerData[] otherPlayers)
        {
            AllPlayers = otherPlayers.Select(data => (PlayerData)data).ToArray();
        }
    }
}
