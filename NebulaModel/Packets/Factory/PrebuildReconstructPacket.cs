namespace NebulaModel.Packets.Factory;

public class PrebuildReconstructPacket
{
    public PrebuildReconstructPacket() { }

    public PrebuildReconstructPacket(int planetId, int prebuildId)
    {
        PlanetId = planetId;
        PrebuildId = prebuildId;
    }

    public int PlanetId { get; set; }
    public int PrebuildId { get; set; }
}
