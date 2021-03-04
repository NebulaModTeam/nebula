using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangeProcessor : IPacketProcessor<PlayerColorChanged>
    {
        public void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdatePlayerColor(packet.PlayerId, packet.Color.ToColor());
        }
    }
}
