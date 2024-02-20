#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

[RegisterPacketProcessor]
public class DFSDeactivateUnitProcessor : PacketProcessor<DFSDeactivateUnitPacket>
{
    protected override void ProcessPacket(DFSDeactivateUnitPacket packet, NebulaConnection conn)
    {
        var spaceSector = GameMain.spaceSector;
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null || packet.EnemyId < 0 || packet.EnemyId >= spaceSector.enemyCursor) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            ref var ptr = ref spaceSector.enemyPool[packet.EnemyId];
            if (ptr.unitId > 0)
            {
                hive.DeactivateUnit(ptr.unitId);
            }
        }
    }
}
