namespace NebulaModel.Packets.Factory.PowerExchanger
{
    public class PowerExchangerStorageUpdatePacket
    {
        public int PowerExchangerIndex { get; set; }
        public int EmptyAccumulatorCount { get; set; }
        public int FullAccumulatorCount { get; set; }
        public int PlanetId { get; set; }

        public PowerExchangerStorageUpdatePacket() { }

        public PowerExchangerStorageUpdatePacket(int powerExchangerIndex, int emptyAccumulatorCount, int fullAccumulatorCount, int planetId)
        {
            PowerExchangerIndex = powerExchangerIndex;
            EmptyAccumulatorCount = emptyAccumulatorCount;
            FullAccumulatorCount = fullAccumulatorCount;
            PlanetId = planetId;
        }
    }
}
