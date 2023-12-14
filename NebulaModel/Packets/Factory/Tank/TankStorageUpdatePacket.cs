namespace NebulaModel.Packets.Factory.Tank;

public class TankStorageUpdatePacket
{
    public TankStorageUpdatePacket() { }

    public TankStorageUpdatePacket(in TankComponent tankComponent, int planetId)
    {
        TankIndex = tankComponent.id;
        FluidId = tankComponent.fluidId;
        FluidCount = tankComponent.fluidCount;
        FluidInc = tankComponent.fluidInc;
        PlanetId = planetId;
    }

    public int TankIndex { get; }
    public int FluidId { get; }
    public int FluidCount { get; }
    public int FluidInc { get; }
    public int PlanetId { get; }
}
