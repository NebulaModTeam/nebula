namespace NebulaModel.Packets.Factory;

public class BuildEntityRequest
{
    public BuildEntityRequest() { }

    public BuildEntityRequest(int planetId, int prebuildId, int authorId, int entityId)
    {
        PlanetId = planetId;
        PrebuildId = prebuildId;
        AuthorId = authorId;
        EntityId = entityId;
    }

    public int PlanetId { get; set; }
    public int PrebuildId { get; set; }
    public int AuthorId { get; set; }
    public int EntityId { get; set; }
}
