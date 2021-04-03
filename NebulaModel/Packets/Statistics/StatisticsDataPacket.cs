namespace NebulaModel.Packets.Statistics
{
    public class StatisticsDataPacket
    {
        public byte[] StatisticsBinaryData { get; set; }

        public StatisticsDataPacket() { }
        public StatisticsDataPacket(byte[] statisticsBinaryData)
        {
            this.StatisticsBinaryData = statisticsBinaryData;
        }
    }
}
