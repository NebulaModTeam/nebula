#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonLaunchDataProcessor : PacketProcessor<DysonLaunchDataPacket>
{
    protected override void ProcessPacket(DysonLaunchDataPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }
        Multiplayer.Session.Launch.ImportPacket(packet);
    }
}
