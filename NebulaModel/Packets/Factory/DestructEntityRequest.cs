namespace NebulaModel.Packets.Factory;

public class DestructEntityRequest
{
    public DestructEntityRequest() { }

    public DestructEntityRequest(int planetId, int objId, int authorId)
    {
        AuthorId = authorId;
        PlanetId = planetId;
        ObjId = objId;
    }

    public int PlanetId { get; set; }
    public int ObjId { get; set; }
    public int AuthorId { get; set; }
}
