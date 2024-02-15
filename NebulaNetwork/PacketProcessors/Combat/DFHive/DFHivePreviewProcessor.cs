#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHivePreviewProcessor : PacketProcessor<DFHivePreviewPacket>
{
    protected override void ProcessPacket(DFHivePreviewPacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            if (packet.OpenPreview)
                hiveSystem.OpenPreview();
            else
                hiveSystem.ClosePreview();
        }
    }
}
