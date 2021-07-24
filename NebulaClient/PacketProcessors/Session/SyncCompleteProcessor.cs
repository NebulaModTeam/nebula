using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
    {
        public override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
            foreach (var playerData in packet.AllPlayers)
            {
                if (playerData.PlayerId != LocalPlayer.PlayerId)
                {
                    SimulatedWorld.SpawnRemotePlayerModel(playerData);
                }
            }

            SimulatedWorld.OnAllPlayersSyncCompleted();
        }
    }
}
