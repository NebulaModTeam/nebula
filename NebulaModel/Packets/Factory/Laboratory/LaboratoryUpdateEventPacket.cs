namespace NebulaModel.Packets.Factory.Laboratory
{
    public class LaboratoryUpdateEventPacket
    {
        public int LabIndex { get; set; }
        public int ProductId { get; set; }
        public int PlanetId { get; set; }

        public LaboratoryUpdateEventPacket() { }

        public LaboratoryUpdateEventPacket(int productId, int labId, int planetId)
        {
            LabIndex = labId;
            ProductId = productId;
            PlanetId = planetId;
        }
    }
}
