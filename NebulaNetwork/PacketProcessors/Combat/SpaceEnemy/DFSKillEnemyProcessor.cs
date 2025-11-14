#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

[RegisterPacketProcessor]
public class DFSKillEnemyProcessor : PacketProcessor<DFSKillEnemyPacket>
{
    protected override void ProcessPacket(DFSKillEnemyPacket packet, NebulaConnection conn)
    {
        var spaceSector = GameMain.spaceSector;
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.OriginAstroId);
        if (hive == null || packet.EnemyId < 0 || packet.EnemyId >= spaceSector.enemyCursor) return;

        ref var ptr = ref spaceSector.enemyPool[packet.EnemyId];
        var killStatistics = spaceSector.skillSystem.killStatistics;
        if (IsHost)
        {
            // Alive, broadcast the event to all clients in the system
            if (ptr.id > 0)
            {
                killStatistics.RegisterStarKillStat(hive.starData.id, ptr.modelIndex);
                spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
            }
            // If the enemy is already dead, that mean the client is behind and kill event has been sent by the server
        }
        else
        {
            using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
            {
                if (ptr.id > 0)
                {
                    killStatistics.RegisterStarKillStat(hive.starData.id, ptr.modelIndex);
                    spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
                }
                else if (ptr.isInvincible) // The marked enemy that waiting for kill approve
                {
                    ptr.id = packet.EnemyId;
                    ptr.isInvincible = false;
                    // kill stat is already registered
                    spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
                }
            }
        }
    }
}
