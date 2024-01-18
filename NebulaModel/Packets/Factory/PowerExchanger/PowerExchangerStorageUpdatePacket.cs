namespace NebulaModel.Packets.Factory.PowerExchanger;

public class PowerExchangerStorageUpdatePacket
{
    public PowerExchangerStorageUpdatePacket() { }

    public PowerExchangerStorageUpdatePacket(int powerExchangerIndex, short emptyAccumulatorCount, short fullAccumulatorCount,
        int planetId, int inc)
    {
        PowerExchangerIndex = powerExchangerIndex;
        EmptyAccumulatorCount = emptyAccumulatorCount;
        FullAccumulatorCount = fullAccumulatorCount;
        PlanetId = planetId;
        Inc = inc;
    }

    public int PowerExchangerIndex { get; set; }
    public short EmptyAccumulatorCount { get; set; }
    public short FullAccumulatorCount { get; set; }
    public int PlanetId { get; set; }
    public int Inc { get; set; }
}
