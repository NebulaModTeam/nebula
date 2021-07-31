using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerDisconnectedProcessor : PacketProcessor<PlayerDisconnected>
    {
        public override void ProcessPacket(PlayerDisconnected packet, NetworkConnection conn)
        {
            SimulatedWorld.DestroyRemotePlayerModel(packet.PlayerId);
        }
    }
}
