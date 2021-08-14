using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerDisconnectedProcessor : PacketProcessor<PlayerDisconnected>
    {
        public override void ProcessPacket(PlayerDisconnected packet, NebulaConnection conn)
        {
            SimulatedWorld.DestroyRemotePlayerModel(packet.PlayerId);
        }
    }
}
