#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class ActivateGroundUnitProcessor : PacketProcessor<ActivateGroundUnitPacket>
{
    protected override void ProcessPacket(ActivateGroundUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        var gameTick = GameMain.gameTick;
        var unitId = factory.enemySystem.ActivateUnit(packet.BaseId, packet.FormId, packet.PortId, gameTick);

        if (unitId == 0)
        {
            // enemyFormation.units[portId] != 1
            NebulaModel.Logger.Log.Warn($"Activate unit {packet.UnitId} didn't success!");
            return;
        }

        ref var buffer = ref factory.enemySystem.units.buffer;
        buffer[unitId].behavior = (EEnemyBehavior)packet.Behavior;
        buffer[unitId].stateTick = packet.StateTick;

        if (packet.UnitId != unitId)
            NebulaModel.Logger.Log.Warn($"Activate unit {packet.UnitId} => {unitId}");
    }
}
