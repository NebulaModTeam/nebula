namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateEventPacket
    {
        public int LabIndex { get; set; }
        public int ProductId { get; set; }
        public int FactoryIndex { get; set; }

        public LaboratoryUpdateEventPacket() { }

        public LaboratoryUpdateEventPacket(int productId, int labId, int factoryIndex)
        {
            LabIndex = labId;
            ProductId = productId;
            FactoryIndex = factoryIndex;
        }
    }
}
