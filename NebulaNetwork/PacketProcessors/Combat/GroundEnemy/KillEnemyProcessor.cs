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
        if (factory == null)
        {
            return;
        }
        if (packet.EnemyId >= factory.enemyPool.Length || packet.EnemyId != factory.enemyPool[packet.EnemyId].id)
        {
            return;
        }

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Skip combatStat.lastImpact syncing for now
            factory.KillEnemyFinally(GameMain.mainPlayer, packet.EnemyId, ref CombatStat.empty);
        }
    }
}
