namespace NebulaModel.Packets.Universe;

public class PlanetMemoUpdatePacket
{
    public PlanetMemoUpdatePacket() { }

    public PlanetMemoUpdatePacket(int planetId, string content, bool hasReminder, short[] colorData = null)
    {
        PlanetId = planetId;
        Content = content;
        HasReminder = hasReminder;
        ColorData = colorData;
    }

    public int PlanetId { get; set; }
    public string Content { get; set; }
    public bool HasReminder { get; set; }
    public short[] ColorData { get; set; }
}
