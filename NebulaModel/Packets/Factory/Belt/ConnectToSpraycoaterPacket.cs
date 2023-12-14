namespace NebulaModel.Packets.Factory.Belt;

public class ConnectToSpraycoaterPacket
{
    public ConnectToSpraycoaterPacket() { }

    public ConnectToSpraycoaterPacket(int spraycoaterId, int cargoBeltId, int incBeltId, int planetId)
    {
        SpraycoaterId = spraycoaterId;
        CargoBeltId = cargoBeltId;
        IncBeltId = incBeltId;
        PlanetId = planetId;
    }

    public int SpraycoaterId { get; }
    public int CargoBeltId { get; }
    public int IncBeltId { get; }
    public int PlanetId { get; }
}
