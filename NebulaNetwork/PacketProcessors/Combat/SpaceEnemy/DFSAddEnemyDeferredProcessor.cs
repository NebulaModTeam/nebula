#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

[RegisterPacketProcessor]
public class DFSAddEnemyDeferredProcessor : PacketProcessor<DFSAddEnemyDeferredPacket>
{
    protected override void ProcessPacket(DFSAddEnemyDeferredPacket packet, NebulaConnection conn)
    {
        var spaceSector = GameMain.spaceSector;
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null || packet.EnemyId <= 0) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            EnemyManager.SetSpaceSectorNextEnemyId(packet.EnemyId);
            var enemyId = spaceSector.CreateEnemyFinal(hive, packet.BuilderIndex, false);
            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFSAddEnemyDeferredPacket mismatch! {packet.EnemyId} => {enemyId}");
            }
        }
    }
}
