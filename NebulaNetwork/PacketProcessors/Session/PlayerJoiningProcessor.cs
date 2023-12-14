#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
public class PlayerJoiningProcessor : PacketProcessor<PlayerJoining>
{
    public override void ProcessPacket(PlayerJoining packet, NebulaConnection conn)
    {
        Multiplayer.Session.NumPlayers = packet.NumPlayers;
        DiscordManager.UpdateRichPresence();
        Multiplayer.Session.World.SpawnRemotePlayerModel(packet.PlayerData);
        Multiplayer.Session.World.OnPlayerJoining(packet.PlayerData.Username);
    }
}
