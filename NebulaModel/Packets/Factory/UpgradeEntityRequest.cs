namespace NebulaModel.Packets.Factory;

public class UpgradeEntityRequest
{
    public UpgradeEntityRequest() { }

    public UpgradeEntityRequest(int planetId, int objId, int upgradeProtoId, int authorId)
    {
        PlanetId = planetId;
        ObjId = objId;
        UpgradeProtoId = upgradeProtoId;
        AuthorId = authorId;
    }

    public int PlanetId { get; }
    public int ObjId { get; }
    public int UpgradeProtoId { get; }
    public int AuthorId { get; }
}
