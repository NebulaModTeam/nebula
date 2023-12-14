namespace NebulaModel.Packets.Planet;

public class PlanetDataResponse
{
    public PlanetDataResponse() { }

    public PlanetDataResponse(int planetId, byte[] planetData)
    {
        PlanetDataID = planetId;
        PlanetDataByte = planetData;
    }

    public int PlanetDataID { get; set; }
    public byte[] PlanetDataByte { get; set; }
}
