#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFSFormationAddUnitProcessor : PacketProcessor<DFSFormationAddUnitPacket>
{
    protected override void ProcessPacket(DFSFormationAddUnitPacket packet, NebulaConnection conn)
    {
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            // Set the next id in EnemyFormation
            var enemyFrom = hive.forms[packet.FormId];
            enemyFrom.vacancies[enemyFrom.vacancyCursor - 1] = packet.PortId;

            var portId = enemyFrom.AddUnit();
#if DEBUG
            if (portId != packet.PortId)
            {
                NebulaModel.Logger.Log.Warn($"DFSFormationAddUnitPacket wrong id {packet.PortId} => {portId}");
            }
#endif
        }
    }
}
