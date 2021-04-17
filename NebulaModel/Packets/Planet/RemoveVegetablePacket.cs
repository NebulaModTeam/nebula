namespace NebulaModel.Packets.Planet
{
    public class RemoveVegetablePacket
    {
        public int FactorytId { get; set; }
        public int VegeId { get; set; }

        public RemoveVegetablePacket() { }

        public RemoveVegetablePacket(int factoryId, int vegeId)
        {
            FactorytId = factoryId;
            VegeId = vegeId;
        }
    }
}
