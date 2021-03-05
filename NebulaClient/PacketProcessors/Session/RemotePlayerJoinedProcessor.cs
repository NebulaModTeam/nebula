using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class RemotePlayerJoinedProcessor : IPacketProcessor<PlayerJoining>
    {
        public void ProcessPacket(PlayerJoining packet, NebulaConnection conn)
        {
            SimulatedWorld.SpawnRemotePlayerModel(packet.PlayerId);
            GameMain.Pause();
        }
    }
}
