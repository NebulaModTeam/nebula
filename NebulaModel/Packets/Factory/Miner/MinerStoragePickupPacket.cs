namespace NebulaModel.Packets.Factory.Miner
{
    public class MinerStoragePickupPacket
    {
        public int MinerIndex { get; set; }

        public MinerStoragePickupPacket() { }

        public MinerStoragePickupPacket(int minerIndex)
        {
            MinerIndex = minerIndex;
        }
    }
}
