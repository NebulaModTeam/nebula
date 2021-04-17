namespace NebulaModel.Packets.Planet
{
    public class RemoveVegetablePacket
    {
        public int FactorytIndex { get; set; }
        public int VegeId { get; set; }

        public RemoveVegetablePacket() { }

        public RemoveVegetablePacket(int factoryId, int vegeId)
        {
            FactorytIndex = factoryId;
            VegeId = vegeId;
        }
    }
}
