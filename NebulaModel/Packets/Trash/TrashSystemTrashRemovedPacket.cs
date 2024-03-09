namespace NebulaModel.Packets.Trash;

public class TrashSystemTrashRemovedPacket
{
    public TrashSystemTrashRemovedPacket() { }

    public TrashSystemTrashRemovedPacket(int trashId, int itemId)
    {
        TrashId = trashId;
        ItemId = itemId;
    }

    public int TrashId { get; set; }
    public int ItemId { get; set; }
}
