namespace NebulaModel.Packets.Planet;

public class PlanetDetailRequest
{
    public PlanetDetailRequest() { }

    public PlanetDetailRequest(int planetID)
    {
        PlanetID = planetID;
    }

    public int PlanetID { get; set; }
}
