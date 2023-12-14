namespace NebulaModel.Packets.Trash;

public class TrashSystemResponseDataPacket
{
    public TrashSystemResponseDataPacket() { }

    public TrashSystemResponseDataPacket(byte[] trashSystemData)
    {
        TrashSystemData = trashSystemData;
    }

    public byte[] TrashSystemData { get; }
}
