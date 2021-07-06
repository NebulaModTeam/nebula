namespace NebulaModel.Packets.Factory.PowerGenerator
{
    public class PowerGeneratorFuelUpdatePacket
    {
        public int PowerGeneratorIndex { get; set; }
        public int FuelId { get; set; }
        public short FuelAmount { get; set; }
        public int PlanetId { get; set; }

        public PowerGeneratorFuelUpdatePacket() { }

        public PowerGeneratorFuelUpdatePacket(int powerGeneratorIndex, int fuelId, short fuelAmount, int planetId)
        {
            PowerGeneratorIndex = powerGeneratorIndex;
            FuelId = fuelId;
            FuelAmount = fuelAmount;
            PlanetId = planetId;
        }
    }
}
