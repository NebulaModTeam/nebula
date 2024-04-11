#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGDeferredCreateEnemyProcessor : PacketProcessor<DFGDeferredCreateEnemyPacket>
{
    protected override void ProcessPacket(DFGDeferredCreateEnemyPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            EnemyManager.SetPlanetFactoryNextEnemyId(factory, packet.EnemyId);
            if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
            if (factory.enemySystem.bases.buffer[packet.BaseId] == null) return;
            var enemyId = factory.CreateEnemyFinal(packet.BaseId, packet.BuilderIndex);

#if DEBUG
            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFGDeferredCreateEnemyPacket enemyId mismatch! {packet.EnemyId} => {enemyId}");
            }
#endif
        }
    }
}
