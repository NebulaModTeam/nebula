namespace NebulaModel.Packets.Trash
{
    public class TrashSystemCorrectionIdPacket
    {
        public int OriginalId { get; set; }
        public int NewId { get; set; }

        public TrashSystemCorrectionIdPacket() { }

        public TrashSystemCorrectionIdPacket(int originalId, int newId)
        {
            OriginalId = originalId;
            NewId = newId;
        }
    }
}
