namespace NebulaModel.Packets.Factory.Tank
{
    public class TankStorageUpdatePacket
    {
        public int TankIndex { get; set; }
        public int FluidId { get; set; }
        public int FluidCount { get; set; }
        public int FluidInc { get; set; }
        public int PlanetId { get; set; }

        public TankStorageUpdatePacket() { }

        public TankStorageUpdatePacket(in TankComponent tankComponent, int planetId)
        {
            TankIndex = tankComponent.id;
            FluidId = tankComponent.fluidId;
            FluidCount = tankComponent.fluidCount;
            FluidInc = tankComponent.fluidInc;
            PlanetId = planetId;
        }
    }
}
