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
public class DFSAddIdleRelayProcessor : PacketProcessor<DFSAddIdleRelayPacket>
{
    protected override void ProcessPacket(DFSAddIdleRelayPacket packet, NebulaConnection conn)
    {
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null || packet.EnemyId <= 0) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            EnemyManager.SetSpaceSectorNextEnemyId(packet.EnemyId);

            // Modify from EnemyHiveSystem.ExecuteDeferredEnemyChange
            ref var dfDock = ref hive.relayDocks[packet.DockIndex % hive.relayDocks.Length];
            var enemyId = hive.sector.CreateEnemyFinal(hive, 8116, hive.hiveAstroId, dfDock.pos, dfDock.rot);
            var dfRelayId = hive.sector.enemyPool[enemyId].dfRelayId;
            var dfrelayComponent = hive.relays.buffer[dfRelayId];
            if (dfrelayComponent != null)
            {
                dfrelayComponent.SetDockIndex(packet.DockIndex);
                hive.AddIdleRelay(dfRelayId);
            }

            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFSAddIdleRelayPacket mismatch! {packet.EnemyId} => {enemyId}");
            }
        }
    }
}
