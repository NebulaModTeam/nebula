using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsDataProcessor : PacketProcessor<StatisticsDataPacket>
    {
        public override void ProcessPacket(StatisticsDataPacket packet, NetworkConnection conn)
        {
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StatisticsBinaryData))
            {
                StatisticsManager.ImportAllHistoryData(reader.BinaryReader);
            }
        }
    }
}
