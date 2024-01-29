#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class ActivateGroundUnitProcessor : PacketProcessor<ActivateGroundUnitPacket>
{
    protected override void ProcessPacket(ActivateGroundUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        var nextUnitId = factory.enemySystem.units.cursor;
        if (factory.enemySystem.units.recycleCursor > 0)
            nextUnitId = factory.enemySystem.units.recycleIds[factory.enemySystem.units.recycleCursor - 1];
        if (nextUnitId != packet.UnitId)
        {
            // UnitId desync. This part is attmept to fix by assigning a correct unitId
            NebulaModel.Logger.Log.Debug($"Activate unit correction {nextUnitId} => {packet.UnitId}, recycle={factory.enemySystem.units.recycleCursor}");
            if (packet.UnitId > factory.enemySystem.units.cursor)
            {
                factory.enemySystem.units.cursor = packet.UnitId;
            }
            else
            {
                factory.enemySystem.units.recycleIds[factory.enemySystem.units.recycleCursor++] = packet.UnitId;
            }
            if (factory.enemySystem.units.cursor >= factory.enemySystem.units.capacity)
            {
                factory.enemySystem.units.SetCapacity(factory.enemySystem.units.capacity * 2);
            }
        }

        var unitId = 0;
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            var gameTick = GameMain.gameTick;
            unitId = factory.enemySystem.ActivateUnit(packet.BaseId, packet.FormId, packet.PortId, gameTick);
        }

        if (unitId == 0)
        {
            // enemyFormation.units[portId] != 1
            NebulaModel.Logger.Log.Warn($"Activate unit {packet.UnitId} didn't success!");
            return;
        }

        ref var enemyUnit = ref factory.enemySystem.units.buffer[unitId];
        enemyUnit.behavior = (EEnemyBehavior)packet.Behavior;
        enemyUnit.stateTick = packet.StateTick;

        if (packet.UnitId != unitId)
        {
            // UnitId desync and beyond fixable. Recommend to reconnect
            NebulaModel.Logger.Log.Warn($"Activate unit wrong id {packet.UnitId} => {enemyUnit.enemyId}");
        }
    }
}
