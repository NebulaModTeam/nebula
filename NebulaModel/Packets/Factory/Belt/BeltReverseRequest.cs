namespace NebulaModel.Packets.Factory.Belt;

public class BeltReverseRequest
{
    public BeltReverseRequest() { }

    public BeltReverseRequest(int beltId, int planetId, int authorId)
    {
        BeltId = beltId;
        PlanetId = planetId;
        AuthorId = authorId;
     }

    public int BeltId { get; set; }
    public int PlanetId { get; set; }
    public int AuthorId { get; set; }
}
