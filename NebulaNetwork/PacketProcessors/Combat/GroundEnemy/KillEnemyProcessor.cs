#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class KillEnemyProcessor : PacketProcessor<KillEnemyPacket>
{
    protected override void ProcessPacket(KillEnemyPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

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
                    // Lower part
                    ref var ptr = ref factory.enemyPool[packet.EnemyId];
                    ptr.id = packet.EnemyId;
                    ptr.isInvincible = false;
                    if (ptr.owner > 0)
                    {
                        factory.enemySystem.NotifyEnemyKilled(ref ptr);
                    }
                    if (ptr.dfGBaseId == 0)
                    {
                        factory.RemoveEnemyWithComponents(packet.EnemyId);
                    }
                }
            }
        }
    }
}
