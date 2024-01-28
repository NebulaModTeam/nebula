#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DeactivateGroundUnitProcessor : PacketProcessor<DeactivateGroundUnitPacket>
{
    protected override void ProcessPacket(DeactivateGroundUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        factory.enemySystem.DeactivateUnit(packet.UnitId);
    }
}
