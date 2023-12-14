namespace NebulaModel.Packets.Factory;

public class EntityBoostSwitchPacket
{
    public EntityBoostSwitchPacket() { }

    public EntityBoostSwitchPacket(int planetId, EBoostEntityType entityType, int id, bool enable)
    {
        PlanetId = planetId;
        EntityType = entityType;
        Id = id;
        Enable = enable;
    }

    public int PlanetId { get; set; }
    public EBoostEntityType EntityType { get; set; }
    public int Id { get; set; }
    public bool Enable { get; set; }
}

public enum EBoostEntityType
{
    ArtificialStar,
    Ejector,
    Silo
}
