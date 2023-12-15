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

    public int PlanetId { get; set; }
    public bool IsReform { get; set; } // true = reform all, false = revert
    public int Type { get; set; }
    public int Color { get; set; }
    public bool Bury { get; set; }
}
