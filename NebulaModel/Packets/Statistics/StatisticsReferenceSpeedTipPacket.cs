namespace NebulaModel.Packets.Statistics;

public class StatisticsReferenceSpeedTipPacket
{
    public StatisticsReferenceSpeedTipPacket() { }

    public StatisticsReferenceSpeedTipPacket(int itemId, int astroFilter, int itemCycle, int productionProtoId, byte[] binaryData)
    {
        ItemId = itemId;
        AstroFilter = astroFilter;
        ItemCycle = itemCycle;
        ProductionProtoId = productionProtoId;
        BinaryData = binaryData;
    }

    public int ItemId { get; set; }
    public int AstroFilter { get; set; }
    public int ItemCycle { get; set; }
    public int ProductionProtoId { get; set; }
    public byte[] BinaryData { get; set; }
}
