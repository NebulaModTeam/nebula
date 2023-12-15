namespace NebulaModel.Packets.Planet;

public class PlanetDataRequest
{
    public PlanetDataRequest() { }

    public PlanetDataRequest(int[] planetIDs)
    {
        PlanetIDs = planetIDs;
    }

    public int[] PlanetIDs { get; set; }
}
