namespace NebulaModel.Packets.Players;

public class RemoveDroneOrdersPacket
{
    public RemoveDroneOrdersPacket() { }

    public RemoveDroneOrdersPacket(int[] queuedEntityIds, int planetId)
    {
        QueuedEntityIds = queuedEntityIds;
        PlanetId = planetId;
    }

    public int[] QueuedEntityIds { get; set; }
    public int PlanetId { get; set; }
}
