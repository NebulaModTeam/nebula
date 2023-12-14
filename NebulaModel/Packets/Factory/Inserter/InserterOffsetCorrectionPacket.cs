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

    public int InserterId { get; }
    public short PickOffset { get; }
    public short InsertOffset { get; }
    public int PlanetId { get; }
}
