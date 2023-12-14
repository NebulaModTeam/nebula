namespace NebulaModel.Packets.Factory.Inserter;

public class InserterOffsetCorrectionPacket
{
    public InserterOffsetCorrectionPacket() { }

    public InserterOffsetCorrectionPacket(int inserterId, short pickOffset, short insertOffset, int planetId)
    {
        InserterId = inserterId;
        PickOffset = pickOffset;
        InsertOffset = insertOffset;
        PlanetId = planetId;
    }

    public int InserterId { get; set; }
    public short PickOffset { get; set; }
    public short InsertOffset { get; set; }
    public int PlanetId { get; set; }
}
