using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerDisconnectedProcessor : IPacketProcessor<PlayerDisconnected>
    {
        public void ProcessPacket(PlayerDisconnected packet, NebulaConnection conn)
        {
            SimulatedWorld.DestroyRemotePlayerModel(packet.PlayerId);
        }
    }
}
