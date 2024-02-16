namespace NebulaModel.Packets.Players;

public class PlayerGiveItemPacket
{
    public PlayerGiveItemPacket() { }

    public PlayerGiveItemPacket(int itemId, int itemCount, int itemInc)
    {
        ItemId = itemId;
        ItemCount = itemCount;
        ItemInc = itemInc;
    }

    public int ItemId { get; set; }
    public int ItemCount { get; set; }
    public int ItemInc { get; set; }
}
