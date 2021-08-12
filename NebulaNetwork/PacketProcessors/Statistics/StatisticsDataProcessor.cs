using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsDataProcessor : PacketProcessor<StatisticsDataPacket>
    {
        public override void ProcessPacket(StatisticsDataPacket packet, NebulaConnection conn)
        {
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StatisticsBinaryData))
            {
                StatisticsManager.ImportAllHistoryData(reader.BinaryReader);
            }
        }
    }
}
