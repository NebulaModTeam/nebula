namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateEventPacket
    {
        public int LabIndex { get; set; }
        public int ProductId { get; set; }

        public LaboratoryUpdateEventPacket() { }

        public LaboratoryUpdateEventPacket(int productId, int labId)
        {
            LabIndex = labId;
            ProductId = productId;
        }
    }
}
