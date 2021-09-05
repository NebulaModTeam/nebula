namespace NebulaModel.Packets.Trash
{
    public class TrashSystemResponseDataPacket
    {
        public byte[] TrashSystemData { get; set; }

        public TrashSystemResponseDataPacket() { }

        public TrashSystemResponseDataPacket(byte[] trashSystemData)
        {
            TrashSystemData = trashSystemData;
        }
    }
}
