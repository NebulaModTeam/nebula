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

        if (IsHost)
        {
            // Alive, broadcast the event to all clients in the system
            if (spaceSector.enemyPool[packet.EnemyId].id > 0)
            {
                spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
            }
            // If the enemy is already dead, that mean the client is behind and kill event has been sent by the server
        }
        else
        {
            using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
            {
                if (spaceSector.enemyPool[packet.EnemyId].id > 0)
                {
                    spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
                }
                else if (spaceSector.enemyPool[packet.EnemyId].isInvincible) // Mark
                {
                    ref var ptr = ref spaceSector.enemyPool[packet.EnemyId];
                    ptr.id = packet.EnemyId;
                    ptr.isInvincible = false;
                    spaceSector.KillEnemyFinal(packet.EnemyId, ref CombatStat.empty);
                }
            }
        }
    }
}
