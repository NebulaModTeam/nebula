namespace NebulaModel.Packets.Players
{
    public class PlayerMechaStat
    {
        public int ItemId { get; set; }
        public int ItemCount { get; set; }

        public PlayerMechaStat() { }
        public PlayerMechaStat(int itemId, int itemCount)
        {
            ItemId = itemId;
            ItemCount = itemCount;
        }
    }
}
