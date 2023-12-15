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

    public int FractionatorId { get; set; }
    public int ProductOutputCount { get; set; }
    public int FluidOutputCount { get; set; }
    public int FluidOutputInc { get; set; }
    public int PlanetId { get; set; }
}
