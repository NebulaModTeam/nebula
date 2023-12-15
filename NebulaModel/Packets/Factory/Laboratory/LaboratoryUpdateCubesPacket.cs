namespace NebulaModel.Packets.Factory.Laboratory;

public class LaboratoryUpdateCubesPacket
{
    public LaboratoryUpdateCubesPacket() { }

    public LaboratoryUpdateCubesPacket(int itemCount, int itemInc, int index, int labIndex, int planetId)
    {
        ItemCount = itemCount;
        ItemInc = itemInc;
        Index = index;
        LabIndex = labIndex;
        PlanetId = planetId;
    }

    public int ItemCount { get; set; }
    public int ItemInc { get; set; }
    public int Index { get; set; }
    public int LabIndex { get; set; }
    public int PlanetId { get; set; }
}
