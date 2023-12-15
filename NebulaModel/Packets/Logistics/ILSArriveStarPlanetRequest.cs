namespace NebulaModel.Packets.Logistics;

public class ILSArriveStarPlanetRequest
{
    public ILSArriveStarPlanetRequest() { }

    public ILSArriveStarPlanetRequest(int starId)
    {
        StarId = starId;
    }

    public int StarId { get; set; }
}
