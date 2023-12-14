namespace NebulaModel.Packets.Statistics;

public class StatisticUpdateDataPacket
{
    public StatisticUpdateDataPacket() { }

    public StatisticUpdateDataPacket(byte[] statisticsBinaryData)
    {
        StatisticsBinaryData = statisticsBinaryData;
    }

    public byte[] StatisticsBinaryData { get; set; }
}
