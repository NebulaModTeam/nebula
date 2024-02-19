namespace NebulaModel.Packets.Factory;

public class PrebuildItemRequiredUpdate
{
    public PrebuildItemRequiredUpdate() { }

    public PrebuildItemRequiredUpdate(int planetId, int prebuildId, int itemCount)
    {
        PlanetId = planetId;
        PrebuildId = prebuildId;
        ItemCount = itemCount;
    }

    public int PlanetId { get; set; }
    public int PrebuildId { get; set; }
    public int ItemCount { get; set; }
}
