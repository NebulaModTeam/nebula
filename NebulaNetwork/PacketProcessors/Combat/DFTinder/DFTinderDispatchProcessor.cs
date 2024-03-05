#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFTinder;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFTinder;

[RegisterPacketProcessor]
public class DFTinderDispatchProcessor : PacketProcessor<DFTinderDispatchPacket>
{
    protected override void ProcessPacket(DFTinderDispatchPacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.OriginHiveAstroId);
        if (hiveSystem == null) return;

        Multiplayer.Session.Enemies.DisplayAstroMessage("Dark Fog seed send out from".Translate(), hiveSystem.starData.astroId);

        ref var tinderComponent = ref hiveSystem.tinders.buffer[packet.TinderId];
        if (tinderComponent.id != packet.TinderId) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            tinderComponent.DispatchFromHive(GameMain.spaceSector, packet.TargetHiveAstroId);
        }
    }
}
