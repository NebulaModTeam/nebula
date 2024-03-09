#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFHive;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFHive;

[RegisterPacketProcessor]
public class DFHiveCreateNewHiveProcessor : PacketProcessor<DFHiveCreateNewHivePacket>
{
    protected override void ProcessPacket(DFHiveCreateNewHivePacket packet, NebulaConnection conn)
    {
        var star = GameMain.galaxy.StarById(packet.StarId);
        if (star == null) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            GameMain.spaceSector.TryCreateNewHive(star);
        }
    }
}
