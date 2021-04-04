namespace NebulaModel.Packets.Statistics
{
    public class StatisticUpdateDataPacket
    {
        public byte[] StatisticsBinaryData { get; set; }

        public StatisticUpdateDataPacket() { }
        public StatisticUpdateDataPacket(byte[] statisticsBinaryData)
        {
            this.StatisticsBinaryData = statisticsBinaryData;
        }
    }
}
