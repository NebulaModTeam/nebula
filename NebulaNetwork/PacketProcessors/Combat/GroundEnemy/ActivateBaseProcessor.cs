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

        var DFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        if (IsHost)
        {
            DFBase.activeTick = 6;
            Multiplayer.Session.Network.SendPacketToStar(packet, factory.planet.star.id);
        }
        else
        {
            DFBase.activeTick = 6;
            return;
        }
    }
}
