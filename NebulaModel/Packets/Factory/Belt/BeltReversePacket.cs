namespace NebulaModel.Packets.Factory.Belt;

public class BeltReversePacket
{
    public BeltReversePacket() { }

    public BeltReversePacket(int beltId, int planetId)
    {
        BeltId = beltId;
        PlanetId = planetId;
    }

    public int BeltId { get; }
    public int PlanetId { get; }
}
