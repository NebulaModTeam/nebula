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

    public int PlanetId { get; }
    public EBoostEntityType EntityType { get; }
    public int Id { get; }
    public bool Enable { get; }
}

public enum EBoostEntityType
{
    ArtificialStar,
    Ejector,
    Silo
}
