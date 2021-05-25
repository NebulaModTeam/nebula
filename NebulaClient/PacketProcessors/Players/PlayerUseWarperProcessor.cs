using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerUseWarperProcessor: IPacketProcessor<PlayerUseWarper>
    {
        public void ProcessPacket(PlayerUseWarper packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerWarpState(packet);
        }
    }
}
