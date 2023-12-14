namespace NebulaModel.Packets.Factory;

public class EntityWarningSwitchPacket
{
    public EntityWarningSwitchPacket() { }

    public EntityWarningSwitchPacket(int planetId, int entityId, bool enable)
    {
        PlanetId = planetId;
        EntityId = entityId;
        Enable = enable;
    }

    public int PlanetId { get; }
    public int EntityId { get; }
    public bool Enable { get; }
}
