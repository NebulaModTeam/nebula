namespace NebulaModel.Packets.Planet;

public class FactoryLoadRequest
{
    public FactoryLoadRequest() { }

    public FactoryLoadRequest(int planetID)
    {
        PlanetID = planetID;
    }

    public int PlanetID { get; set; }
}
