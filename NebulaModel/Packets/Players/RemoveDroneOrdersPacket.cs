namespace NebulaModel.Packets.Players;

public class RemoveDroneOrdersPacket
{
    public RemoveDroneOrdersPacket() { }

    public RemoveDroneOrdersPacket(int[] queuedEntityIds)
    {
        QueuedEntityIds = queuedEntityIds;
    }

    public int[] QueuedEntityIds { get; set; }
}
