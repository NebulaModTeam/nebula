namespace NebulaModel.Packets.Factory.PowerGenerator;

public class PowerGeneratorFuelUpdatePacket
{
    public PowerGeneratorFuelUpdatePacket() { }

    public PowerGeneratorFuelUpdatePacket(int powerGeneratorIndex, int fuelId, short fuelAmount, short fuelInc, int planetId)
    {
        PowerGeneratorIndex = powerGeneratorIndex;
        FuelId = fuelId;
        FuelAmount = fuelAmount;
        FuelInc = fuelInc;
        PlanetId = planetId;
    }

    public int PowerGeneratorIndex { get; set; }
    public int FuelId { get; set; }
    public short FuelAmount { get; set; }
    public short FuelInc { get; set; }
    public int PlanetId { get; set; }
}
