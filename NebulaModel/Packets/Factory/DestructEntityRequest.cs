namespace NebulaModel.Packets.Factory;

public class DestructEntityRequest
{
    public DestructEntityRequest() { }

    public DestructEntityRequest(int planetId, int objId, int protoId, int authorId)
    {
        AuthorId = authorId;
        PlanetId = planetId;
        ProtoId = protoId;
        ObjId = objId;
    }

    public int PlanetId { get; set; }
    public int ObjId { get; set; }
    public int ProtoId { get; set; }
    public int AuthorId { get; set; }
}
