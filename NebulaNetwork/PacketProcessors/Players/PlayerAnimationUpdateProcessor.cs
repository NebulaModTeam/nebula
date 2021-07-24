using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : PacketProcessor<PlayerAnimationUpdate>
    {
        public override void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerAnimation(packet);
        }
    }
}
