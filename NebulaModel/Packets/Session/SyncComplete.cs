using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class SyncComplete
    {
        public PlayerData[] AllPlayers { get; set; }

        public SyncComplete() { AllPlayers = new PlayerData[] { }; }
        public SyncComplete(PlayerData[] otherPlayers)
        {
            AllPlayers = otherPlayers;
        }
    }
}
