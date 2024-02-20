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
public class DFSAddIdleTinderProcessor : PacketProcessor<DFSAddIdleTinderPacket>
{
    protected override void ProcessPacket(DFSAddIdleTinderPacket packet, NebulaConnection conn)
    {
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null || packet.EnemyId <= 0) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            EnemyManager.SetSpaceSectorNextEnemyId(packet.EnemyId);

            // Modify from EnemyHiveSystem.ExecuteDeferredEnemyChange
            ref var dfDock = ref hive.tinderDocks[packet.DockIndex % hive.tinderDocks.Length];
            var enemyId = hive.sector.CreateEnemyFinal(hive, 8119, hive.hiveAstroId, dfDock.pos, dfDock.rot);
            var dfTinderId = hive.sector.enemyPool[enemyId].dfTinderId;
            ref var dFTinderComponent = ref hive.tinders.buffer[dfTinderId];
            if (dfTinderId > 0)
            {
                dFTinderComponent.SetDockIndex(hive, packet.DockIndex);
                hive.AddIdleTinder(dfTinderId);
            }

            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFSAddIdleTinderPacket mismatch! {packet.EnemyId} => {enemyId}");
            }
        }
    }
}
