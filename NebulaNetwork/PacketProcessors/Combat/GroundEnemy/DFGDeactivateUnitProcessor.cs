#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGDeactivateUnitProcessor : PacketProcessor<DFGDeactivateUnitPacket>
{
    protected override void ProcessPacket(DFGDeactivateUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            ref var ptr = ref factory.enemyPool[packet.EnemyId];
            if (ptr.isInvincible && ptr.id != packet.EnemyId)
            {
                // Restore the pending to kill enemy back to normal state
                ptr.id = packet.EnemyId;
                ptr.isInvincible = false;
            }
            factory.enemySystem.DeactivateUnit(ptr.unitId);
        }
    }
}
