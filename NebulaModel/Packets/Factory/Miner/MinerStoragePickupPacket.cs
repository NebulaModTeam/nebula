namespace NebulaModel.Packets.Factory.Miner
{
    public class MinerStoragePickupPacket
    {
        public int MinerIndex { get; set; }
        public int PlanetId { get; set; }

        public MinerStoragePickupPacket() { }

        public MinerStoragePickupPacket(int minerIndex, int planetId)
        {
            MinerIndex = minerIndex;
            PlanetId = planetId;
        }
    }
}
