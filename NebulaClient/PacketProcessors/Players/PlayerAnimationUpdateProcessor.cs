using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : IPacketProcessor<PlayerAnimationUpdate>
    {
        public void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerAnimation(packet);
        }
    }
}
