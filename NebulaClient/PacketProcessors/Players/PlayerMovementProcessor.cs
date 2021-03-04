using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : IPacketProcessor<PlayerMovement>
    {
        public void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerPosition(packet);
        }
    }
}
