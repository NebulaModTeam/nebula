#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

[RegisterPacketProcessor]
public class DFSActivateUnitProcessor : PacketProcessor<DFSActivateUnitPacket>
{
    protected override void ProcessPacket(DFSActivateUnitPacket packet, NebulaConnection conn)
    {
        var spaceSector = GameMain.spaceSector;
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null || packet.EnemyId <= 0) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            EnemyManager.SetSpaceSectorNextEnemyId(packet.EnemyId);
            var gameTick = GameMain.gameTick;

            // Modify EnemyDFHiveSystem.ActivateUnit
            var enemyFormation = hive.forms[packet.FormId];
            enemyFormation.units[packet.PortId] = 2;
            var protoId = 8113 - packet.FormId;
            var formTicks = (hive.starData.seed + gameTick) % 1512000L;
            var enemyId = hive.sector.CreateEnemyFinal(hive, protoId, hive.hiveAstroId, packet.PortId, (int)formTicks);
            var unitId = hive.sector.enemyPool[enemyId].unitId;

            ref var enemyUnit = ref hive.units.buffer[unitId];
            enemyUnit.behavior = (EEnemyBehavior)packet.Behavior;
            enemyUnit.stateTick = packet.StateTick;

#if DEBUG
            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFSActivateUnitPacket enemyId mismatch! {packet.EnemyId} => {enemyId}");
            }
            if (unitId != packet.UnitId)
            {
                Log.Warn($"DFSActivateUnitPacket unitId mismatch! {packet.UnitId} => {unitId}");
            }
#endif
        }
    }
}
