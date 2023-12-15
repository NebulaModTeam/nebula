namespace NebulaModel.Packets.Players;

public class PlayerMechaStat
{
    public PlayerMechaStat() { }

    public PlayerMechaStat(int itemId, int itemCount)
    {
        ItemId = itemId;
        ItemCount = itemCount;
    }

    public int ItemId { get; set; }
    public int ItemCount { get; set; }
}
