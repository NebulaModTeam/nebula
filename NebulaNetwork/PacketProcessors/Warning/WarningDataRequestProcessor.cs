#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Warning;

[RegisterPacketProcessor]
internal class WarningDataRequestProcessor : PacketProcessor<WarningDataRequest>
{
    protected override void ProcessPacket(WarningDataRequest packet, NebulaConnection conn)
    {
        Multiplayer.Session.Warning.HandleRequest(packet, conn);
    }
}
