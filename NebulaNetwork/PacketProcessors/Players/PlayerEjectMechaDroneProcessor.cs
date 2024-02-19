#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerEjectMechaDroneProcessor : PacketProcessor<PlayerEjectMechaDronePacket>
{
    protected override void ProcessPacket(PlayerEjectMechaDronePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        Multiplayer.Session.Drones.EjectMechaDroneFromOtherPlayer(packet);
    }
}
