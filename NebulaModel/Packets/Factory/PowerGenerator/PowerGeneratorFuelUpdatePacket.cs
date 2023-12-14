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

    public int PowerGeneratorIndex { get; }
    public int FuelId { get; }
    public short FuelAmount { get; }
    public short FuelInc { get; }
    public int PlanetId { get; }
}
