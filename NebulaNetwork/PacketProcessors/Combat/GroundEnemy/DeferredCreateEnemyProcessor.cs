#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DeferredCreateEnemyProcessor : PacketProcessor<DeferredCreateEnemyPacket>
{
    protected override void ProcessPacket(DeferredCreateEnemyPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            var enemyId = factory.CreateEnemyFinal(packet.BaseId, packet.BuilderIndex);
            if (enemyId != packet.EnemyId)
            {
                NebulaModel.Logger.Log.Warn($"DeferredCreateEnemyPacket wrong id {packet.EnemyId} => {enemyId}");
            }
        }
    }
}
