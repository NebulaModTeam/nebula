using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangeProcessor : PacketProcessor<PlayerColorChanged>
    {
        public override void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdatePlayerColor(packet.PlayerId, packet.Color);
        }
    }
}
