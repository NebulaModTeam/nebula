namespace NebulaModel.Packets.Factory.Ejector;

public class EjectorOrbitUpdatePacket
{
    public EjectorOrbitUpdatePacket() { }

    public EjectorOrbitUpdatePacket(int ejectorIndex, int newOrbitIndex, int planetId)
    {
        EjectorIndex = ejectorIndex;
        NewOrbitIndex = newOrbitIndex;
        PlanetId = planetId;
    }

    public int EjectorIndex { get; set; }
    public int NewOrbitIndex { get; set; }
    public int PlanetId { get; set; }
}
