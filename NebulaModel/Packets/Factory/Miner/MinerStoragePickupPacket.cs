namespace NebulaModel.Packets.Factory.Miner
{
    public class MinerStoragePickupPacket
    {
        public int MinerIndex { get; set; }
        public int FactoryIndex { get; set; }

        public MinerStoragePickupPacket() { }

        public MinerStoragePickupPacket(int minerIndex, int factoryIndex)
        {
            MinerIndex = minerIndex;
            FactoryIndex = factoryIndex;
        }
    }
}
