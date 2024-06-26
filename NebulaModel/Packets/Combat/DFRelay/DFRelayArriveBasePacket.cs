namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayArriveBasePacket
{
    public DFRelayArriveBasePacket() { }

    public DFRelayArriveBasePacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
        HiveRtseed = dFRelay.hive.rtseed;

        var factory = dFRelay.hive.galaxy.astrosFactory[dFRelay.targetAstroId];
        if (factory != null)
        {
            NextGroundEnemyId = factory.enemyRecycleCursor > 0 ? factory.enemyRecycle[factory.enemyRecycleCursor - 1] : factory.enemyCursor;
            HasFactory = factory.entityCount > 0;
        }
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
    public int HiveRtseed { get; set; }
    public int NextGroundEnemyId { get; set; }
    public bool HasFactory { get; set; }
}
