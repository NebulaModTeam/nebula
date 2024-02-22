#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHiveOpenPreviewProcessor : PacketProcessor<DFHiveOpenPreviewPacket>
{
    protected override void ProcessPacket(DFHiveOpenPreviewPacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            if (packet.EnemyCursor != -1)
            {
                EnemyManager.SetSpaceSectorRecycle(packet.EnemyCursor, packet.EnemyRecycle);
            }
            hiveSystem.OpenPreview();
        }
    }
}
