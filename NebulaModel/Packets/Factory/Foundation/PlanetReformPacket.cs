namespace NebulaModel.Packets.Factory.Foundation;

public class PlanetReformPacket
{
    public PlanetReformPacket() { }

    public PlanetReformPacket(int planetId, bool isReform, int type = 0, int color = 0, bool bury = false)
    {
        PlanetId = planetId;
        IsReform = isReform;
        Type = type;
        Color = color;
        Bury = bury;
    }

    public int PlanetId { get; }
    public bool IsReform { get; } // true = reform all, false = revert
    public int Type { get; }
    public int Color { get; }
    public bool Bury { get; }
}
