namespace NebulaModel.Packets.Planet;

public class PlanetDataResponse
{
    public PlanetDataResponse() { }

    public PlanetDataResponse(int planetId, byte[] planetData)
    {
        PlanetDataID = planetId;
        PlanetDataByte = planetData;
    }

    public int PlanetDataID { get; }
    public byte[] PlanetDataByte { get; }
}
