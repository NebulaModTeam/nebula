using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerJoiningProcessor : PacketProcessor<PlayerJoining>
    {
        public override void ProcessPacket(PlayerJoining packet, NebulaConnection conn)
        {
            SimulatedWorld.Instance.SpawnRemotePlayerModel(packet.PlayerData);
            SimulatedWorld.Instance.OnPlayerJoining();
        }
    }
}
