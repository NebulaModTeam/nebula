namespace NebulaModel.Packets.Statistics;

public class StatisticsExtraDataPacket
{
    public StatisticsExtraDataPacket() { }

    public StatisticsExtraDataPacket(int factoryCount, byte[] binaryData)
    {
        FactoryCount = factoryCount;
        BinaryData = binaryData;
    }

    public int FactoryCount { get; set; }
    public byte[] BinaryData { get; set; }
}
