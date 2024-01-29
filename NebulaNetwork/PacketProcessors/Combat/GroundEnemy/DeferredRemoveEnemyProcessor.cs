#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DeferredRemoveEnemyProcessor : PacketProcessor<DeferredRemoveEnemyPacket>
{
    protected override void ProcessPacket(DeferredRemoveEnemyPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        factory.RemoveEnemyFinal(packet.EnemyId);
    }
}
