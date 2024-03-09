#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHiveUnderAttackProcessor : PacketProcessor<DFHiveUnderAttackRequest>
{
    protected override void ProcessPacket(DFHiveUnderAttackRequest packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        if (packet.Radius == 0)
        {
            hiveSystem.UnderAttack();
        }
        else
        {
            var centerPos = new VectorLF3(packet.CenterUPos.x, packet.CenterUPos.y, packet.CenterUPos.z);
            hiveSystem.UnderAttack(ref centerPos, packet.Radius);
        }
    }
}
