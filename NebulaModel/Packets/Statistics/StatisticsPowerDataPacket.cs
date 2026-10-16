namespace NebulaModel.Packets.Statistics;

/// <summary>
/// Carries the vanilla power statistics (generation capacities, consumption demands,
/// totals and build counts) so clients can render the unmodified Power Dashboard.
/// </summary>
public class StatisticsPowerDataPacket
{
    public StatisticsPowerDataPacket() { }

    public StatisticsPowerDataPacket(int astroFilter, byte[] powerBinaryData)
    {
        AstroFilter = astroFilter;
        PowerBinaryData = powerBinaryData;
    }

    public int AstroFilter { get; set; }
    public byte[] PowerBinaryData { get; set; }
}
