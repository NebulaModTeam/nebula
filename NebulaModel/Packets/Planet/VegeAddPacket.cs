namespace NebulaModel.Packets.Planet;

public class VegeAddPacket
{
    public VegeAddPacket() { }

    public VegeAddPacket(int planetId, bool isVein, byte[] data)
    {
        PlanetId = planetId;
        IsVein = isVein;
        Data = data;
    }

    public int PlanetId { get; set; }
    public bool IsVein { get; set; }
    public byte[] Data { get; set; }
}
