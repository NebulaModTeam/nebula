namespace NebulaModel.Packets.Statistics;

public class StatisticsDataPacket
{
    public StatisticsDataPacket() { }

    public StatisticsDataPacket(byte[] statisticsBinaryData)
    {
        StatisticsBinaryData = statisticsBinaryData;
    }

    public byte[] StatisticsBinaryData { get; set; }
}
