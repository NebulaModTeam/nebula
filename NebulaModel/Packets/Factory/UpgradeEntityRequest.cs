namespace NebulaModel.Packets.Factory;

public class UpgradeEntityRequest
{
    public UpgradeEntityRequest() { }

    public UpgradeEntityRequest(int planetId, int objId, int originProtoId, int upgradeProtoId, int authorId)
    {
        PlanetId = planetId;
        ObjId = objId;
        OriginProtoId = originProtoId;
        UpgradeProtoId = upgradeProtoId;
        AuthorId = authorId;
    }

    public int PlanetId { get; set; }
    public int ObjId { get; set; }
    public int OriginProtoId { get; set; }
    public int UpgradeProtoId { get; set; }
    public int AuthorId { get; set; }
}
