#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.GroundEnemy;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHiveUpdateStatusProcessor : PacketProcessor<DFHiveUpdateStatusPacket>
{
    protected override void ProcessPacket(DFHiveUpdateStatusPacket packet, NebulaConnection conn)
    {
        var hive = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null) return;

        ref var evolveData = ref hive.evolve;
        evolveData.threat = packet.Threat;
        if (evolveData.level != packet.Level)
        {
            evolveData.expp = 0;
        }
        evolveData.level = packet.Level;
        evolveData.expl = packet.Expl;
        evolveData.expf = packet.Expf;
    }
}
