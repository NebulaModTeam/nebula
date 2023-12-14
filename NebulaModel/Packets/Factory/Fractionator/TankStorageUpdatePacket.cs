namespace NebulaModel.Packets.Factory.Fractionator;

public class FractionatorStorageUpdatePacket
{
    public FractionatorStorageUpdatePacket() { }

    public FractionatorStorageUpdatePacket(in FractionatorComponent fractionatorComponent, int planetId)
    {
        FractionatorId = fractionatorComponent.id;
        ProductOutputCount = fractionatorComponent.productOutputCount;
        FluidOutputCount = fractionatorComponent.fluidOutputCount;
        FluidOutputInc = fractionatorComponent.fluidOutputInc;
        PlanetId = planetId;
    }

    public int FractionatorId { get; }
    public int ProductOutputCount { get; }
    public int FluidOutputCount { get; }
    public int FluidOutputInc { get; }
    public int PlanetId { get; }
}
