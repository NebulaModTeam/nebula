using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerJoiningProcessor : PacketProcessor<PlayerJoining>
    {
        public override void ProcessPacket(PlayerJoining packet, NetworkConnection conn)
        {
            SimulatedWorld.SpawnRemotePlayerModel(packet.PlayerData);
            SimulatedWorld.OnPlayerJoining();
        }
    }
}
