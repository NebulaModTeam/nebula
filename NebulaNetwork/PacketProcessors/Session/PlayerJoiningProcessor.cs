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
            Multiplayer.Session.World.SpawnRemotePlayerModel(packet.PlayerData);
            Multiplayer.Session.World.OnPlayerJoining();
        }
    }
}
