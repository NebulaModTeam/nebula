#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class ActivateBaseProcessor : PacketProcessor<ActivateBasePacket>
{
    protected override void ProcessPacket(ActivateBasePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        if (IsHost) // Forward and approve the active base event
        {
            Multiplayer.Session.Network.SendPacketToStar(packet, factory.planet.star.id);
        }
        var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        dFBase.activeTick = 3;
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            dFBase.ActiveAllUnit(GameMain.gameTick);
        }
    }
}
