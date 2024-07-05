#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGKillEnemyProcessor : PacketProcessor<DFGKillEnemyPacket>
{
    protected override void ProcessPacket(DFGKillEnemyPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null || packet.EnemyId >= factory.enemyPool.Length) return;

        if (IsHost)
        {
            // Alive, broadcast the event to all clients in the system
            if (factory.enemyPool[packet.EnemyId].id > 0)
            {
                factory.KillEnemyFinally(GameMain.mainPlayer, packet.EnemyId, ref CombatStat.empty);
            }
            // If the enemy is already dead, that mean the client is behind and kill event has been sent by the server
        }
        else
        {
            using (Multiplayer.Session.Combat.IsIncomingRequest.On())
            {
                if (factory.enemyPool[packet.EnemyId].id > 0)
                {
                    factory.KillEnemyFinally(GameMain.mainPlayer, packet.EnemyId, ref CombatStat.empty);
                }
                else if (factory.enemyPool[packet.EnemyId].isInvincible) // Mark
                {
                    ref var ptr = ref factory.enemyPool[packet.EnemyId];
                    ptr.id = packet.EnemyId;
                    ptr.isInvincible = false;
                    factory.KillEnemyFinally(GameMain.mainPlayer, packet.EnemyId, ref CombatStat.empty);
                }
            }
        }
    }
}
