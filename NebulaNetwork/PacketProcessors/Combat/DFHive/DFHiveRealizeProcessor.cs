#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHiveRealizeProcessor : PacketProcessor<DFHiveRealizePacket>
{
    protected override void ProcessPacket(DFHiveRealizePacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            hiveSystem.Realize();
        }
    }
}
