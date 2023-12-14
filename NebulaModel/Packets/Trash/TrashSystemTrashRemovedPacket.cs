namespace NebulaModel.Packets.Trash;

public class TrashSystemTrashRemovedPacket
{
    public TrashSystemTrashRemovedPacket() { }

    public TrashSystemTrashRemovedPacket(int trashId)
    {
        TrashId = trashId;
    }

    public int TrashId { get; }
}
