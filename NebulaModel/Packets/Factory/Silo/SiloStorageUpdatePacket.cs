namespace NebulaModel.Packets.Factory.Silo
{
    public class SiloStorageUpdatePacket
    {
        public int SiloIndex { get; set; }
        public int NewRocketsAmount { get; set; }
        public int FactoryIndex { get; set; }

        public SiloStorageUpdatePacket() { }
        public SiloStorageUpdatePacket(int siloIndex, int newRocketsAmount, int factoryIndex)
        {
            SiloIndex = siloIndex;
            NewRocketsAmount = newRocketsAmount;
            FactoryIndex = factoryIndex;
        }
    }
}
