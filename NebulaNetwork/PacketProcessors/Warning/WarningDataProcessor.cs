#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Warning;

[RegisterPacketProcessor]
internal class WarningDataProcessor : PacketProcessor<WarningDataPacket>
{
    public override void ProcessPacket(WarningDataPacket packet, NebulaConnection conn)
    {
        Multiplayer.Session.Warning.TickData = packet.Tick;
        using (var reader = new BinaryUtils.Reader(packet.BinaryData))
        {
            Multiplayer.Session.Warning.ImportBinaryData(reader.BinaryReader, packet.ActiveWarningCount);
        }
    }
}
