namespace NebulaModel.Packets.Factory;

public class KillEntityRequest
{
    public KillEntityRequest() { }

    public KillEntityRequest(int planetId, int objId)
    {
        PlanetId = planetId;
        ObjId = objId;
    }

    public int PlanetId { get; set; }
    public int ObjId { get; set; }
}
