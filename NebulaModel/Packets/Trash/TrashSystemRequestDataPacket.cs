namespace NebulaModel.Packets.Trash
{
    public class TrashSystemRequestDataPacket
    {
        public int StarId { get; set; }

        public TrashSystemRequestDataPacket() { }

        public TrashSystemRequestDataPacket(int starId)
        {
            StarId = starId;
        }
    }
}
