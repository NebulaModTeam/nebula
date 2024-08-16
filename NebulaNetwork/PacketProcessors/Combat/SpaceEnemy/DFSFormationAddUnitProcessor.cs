#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

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

            if (enemyFrom.vacancyCursor <= 0)
            {
                // If vacancyCursor desync, abort the AddUnit action
                Log.Warn($"DFSFormationAddUnitPacket vacancyCursor desync. HiveAstroId={packet.HiveAstroId} FormId={packet.FormId}");
                return;
            }
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
