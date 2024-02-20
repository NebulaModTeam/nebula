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

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            var portId = hive.forms[packet.FormId].AddUnit();
#if DEBUG
            if (portId != packet.PortId)
            {
                NebulaModel.Logger.Log.Warn($"DFSFormationAddUnitPacket wrong id {packet.PortId} => {portId}");
            }
#endif
        }
    }
}
