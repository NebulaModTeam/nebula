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

    public int PlanetId { get; }
    public bool IsVein { get; }
    public byte[] Data { get; }
}
