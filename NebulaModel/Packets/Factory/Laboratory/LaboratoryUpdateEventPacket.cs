namespace NebulaModel.Packets.Factory.Laboratory;

public class LaboratoryUpdateEventPacket
{
    public LaboratoryUpdateEventPacket() { }

    public LaboratoryUpdateEventPacket(int productId, int labId, int planetId)
    {
        LabIndex = labId;
        ProductId = productId;
        PlanetId = planetId;
    }

    public int LabIndex { get; }
    public int ProductId { get; }
    public int PlanetId { get; }
}
