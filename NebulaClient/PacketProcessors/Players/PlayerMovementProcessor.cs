using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : PacketProcessor<PlayerMovement>
    {
        public override void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerPosition(packet);
        }
    }
}
