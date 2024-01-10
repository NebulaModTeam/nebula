#region

using System.IO;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsDataProcessor : PacketProcessor<StatisticsDataPacket>
{
    protected override void ProcessPacket(StatisticsDataPacket packet, NebulaConnection conn)
    {
        using var reader = new BinaryUtils.Reader(packet.StatisticsBinaryData);
        Multiplayer.Session.Statistics.ImportAllData(reader.BinaryReader);
    }
}
