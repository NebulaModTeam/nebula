namespace NebulaModel.Packets.Factory.Miner;

public class MinerStoragePickupPacket
{
    public MinerStoragePickupPacket() { }

    public MinerStoragePickupPacket(int minerIndex, int planetId)
    {
        MinerIndex = minerIndex;
        PlanetId = planetId;
    }

    public int MinerIndex { get; }
    public int PlanetId { get; }
}
