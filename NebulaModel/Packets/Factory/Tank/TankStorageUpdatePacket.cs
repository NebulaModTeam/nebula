namespace NebulaModel.Packets.Factory.Tank
{
    public class TankStorageUpdatePacket
    {
        public int TankIndex { get; set; }
        public int FluidId { get; set; }
        public int FluidCount { get; set; }
        public int PlanetId { get; set; }

        public TankStorageUpdatePacket() { }

        public TankStorageUpdatePacket(int tankIndex, int fluidId, int fluidCount, int planetId)
        {
            TankIndex = tankIndex;
            FluidId = fluidId;
            FluidCount = fluidCount;
            PlanetId = planetId;
        }
    }
}
