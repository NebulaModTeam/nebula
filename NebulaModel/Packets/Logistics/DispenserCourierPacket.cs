namespace NebulaModel.Packets.Logistics
{
    public class DispenserCourierPacket
    {
        public int PlayerId { get; set; }
        public int DispenserId { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }

        public DispenserCourierPacket() { }
        public DispenserCourierPacket(int playerId, int dispenserId, int itemId, int itemCount)
        {
            PlayerId = playerId;
            DispenserId = dispenserId;
            ItemId = itemId;
            ItemCount = itemCount;
        }
    }
}
