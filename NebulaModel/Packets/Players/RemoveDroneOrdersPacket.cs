namespace NebulaHost.PacketProcessors.Players
{
    public class RemoveDroneOrdersPacket
    {
        public int[] QueuedEntityIds { get; set; }

        public RemoveDroneOrdersPacket() { }

        public RemoveDroneOrdersPacket(int[] queuedEntityIds)
        {
            QueuedEntityIds = queuedEntityIds;
        }
    }
}
