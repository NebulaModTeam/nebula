namespace NebulaModel.Packets.Planet
{
    public class VegeMinedPacket
    {
        public int FactoryIndex { get; set; }
        public int VegeId { get; set; }
        public int Amount { get; set; } // the current amount, if 0 remove vege
        public bool IsVein { get; set; }

        public VegeMinedPacket() { }

        public VegeMinedPacket(int factoryId, int vegeId, int amount, bool isVein)
        {
            FactoryIndex = factoryId;
            VegeId = vegeId;
            Amount = amount;
            IsVein = isVein;
        }
    }
}
