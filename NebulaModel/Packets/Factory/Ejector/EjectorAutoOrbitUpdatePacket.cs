namespace NebulaModel.Packets.Factory.Ejector;

public class EjectorAutoOrbitUpdatePacket
{
    public EjectorAutoOrbitUpdatePacket() { }

    public EjectorAutoOrbitUpdatePacket(int ejectorIndex, bool autoOrbit, int planetId)
    {
        EjectorIndex = ejectorIndex;
        AutoOrbit = autoOrbit;
        PlanetId = planetId;
    }

    public int EjectorIndex { get; set; }
    public bool AutoOrbit { get; set; }
    public int PlanetId { get; set; }
}
