using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using System.IO;
using LZ4;
using System.IO.Compression;
using NebulaModel.Logger;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;

namespace NebulaClient.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsDataProcessor : IPacketProcessor<StatisticsDataPacket>
    {
        public void ProcessPacket(StatisticsDataPacket packet, NebulaConnection conn)
        {
            Log.Info($"Parsing Statistics data from the server.");
            using (MemoryStream ms = new MemoryStream(packet.StatisticsBinaryData))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                StatisticsManager.ImporAllHistorytData(br);
            }
        }
    }
}
