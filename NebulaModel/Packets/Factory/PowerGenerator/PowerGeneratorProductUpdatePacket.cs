namespace NebulaModel.Packets.Factory.PowerGenerator;

public class PowerGeneratorProductUpdatePacket
{
    public PowerGeneratorProductUpdatePacket() { }

    public PowerGeneratorProductUpdatePacket(in PowerGeneratorComponent powerGenerator, int planetId)
    {
        PowerGeneratorIndex = powerGenerator.id;
        ProductCount = powerGenerator.productCount;
        PlanetId = planetId;
    }

    public int PowerGeneratorIndex { get; }
    public float ProductCount { get; }
    public int PlanetId { get; }
}
