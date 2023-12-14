namespace NebulaModel.Packets.Factory.PowerExchanger;

public class PowerExchangerStorageUpdatePacket
{
    public PowerExchangerStorageUpdatePacket() { }

    public PowerExchangerStorageUpdatePacket(int powerExchangerIndex, int emptyAccumulatorCount, int fullAccumulatorCount,
        int planetId)
    {
        PowerExchangerIndex = powerExchangerIndex;
        EmptyAccumulatorCount = emptyAccumulatorCount;
        FullAccumulatorCount = fullAccumulatorCount;
        PlanetId = planetId;
    }

    public int PowerExchangerIndex { get; }
    public int EmptyAccumulatorCount { get; }
    public int FullAccumulatorCount { get; }
    public int PlanetId { get; }
}
