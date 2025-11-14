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

        ref var ptr = ref factory.enemyPool[packet.EnemyId];
        var killStatistics = GameMain.data.spaceSector.skillSystem.killStatistics;
        if (IsHost)
        {
            // Alive, broadcast the event to all clients in the system
            if (ptr.id > 0)
            {
                killStatistics.RegisterFactoryKillStat(factory.index, ptr.modelIndex);
                factory.KillEnemyFinally(packet.EnemyId, ref CombatStat.empty);
            }
            // If the enemy is already dead, that mean the client is behind and kill event has been sent by the server
        }
        else
        {
            using (Multiplayer.Session.Combat.IsIncomingRequest.On())
            {
                if (ptr.id > 0)
                {
                    killStatistics.RegisterFactoryKillStat(factory.index, ptr.modelIndex);
                    factory.KillEnemyFinally(packet.EnemyId, ref CombatStat.empty);
                }
                else if (ptr.isInvincible) // The marked enemy that waiting for kill approve
                {
                    ptr.id = packet.EnemyId;
                    ptr.isInvincible = false;
                    // kill stat is already registered
                    factory.KillEnemyFinally(packet.EnemyId, ref CombatStat.empty);
                }
            }
        }
    }
}
