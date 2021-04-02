using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;

namespace NebulaClient.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsDataProcessor : IPacketProcessor<StatisticsDataPacket>
    {
        public void ProcessPacket(StatisticsDataPacket packet, NebulaConnection conn)
        {
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StatisticsBinaryData))
            {
                StatisticsManager.ImporAllHistorytData(reader.BinaryReader);
            }
        }
    }
}
