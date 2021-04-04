namespace NebulaModel.Packets.Trash
{
    public class TrashSystemTrashRemovedPacket
    {
        public int TrashId { get; set; }

        public TrashSystemTrashRemovedPacket() { }

        public TrashSystemTrashRemovedPacket(int trashId)
        {
            this.TrashId = trashId;
        }
    }
}
