namespace NebulaModel.Packets.Factory.Belt;

public class BeltReverseRequestPacket
{
    public BeltReverseRequestPacket() { }

    public BeltReverseRequestPacket(int beltId, int planetId, int authorId)
    {
        BeltId = beltId;
        PlanetId = planetId;
        AuthorId = authorId;
    }

    public int BeltId { get; set; }
    public int PlanetId { get; set; }
    public int AuthorId { get; set; }
}
