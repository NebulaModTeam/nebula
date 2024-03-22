using System;
using NebulaAPI.DataStructures;

namespace NebulaModel.Packets.Combat.DFRelay;

public class DFRelayRealizePlanetBasePacket
{
    public DFRelayRealizePlanetBasePacket() { }

    public DFRelayRealizePlanetBasePacket(in DFRelayComponent dFRelay)
    {
        HiveAstroId = dFRelay.hiveAstroId;
        RelayId = dFRelay.id;
        RelayNeutralizedCounter = dFRelay.hive.relayNeutralizedCounter;
        HiveSeed = dFRelay.hive.seed;
        HiveRtseed = dFRelay.hive.rtseed;
        TargetAstroId = dFRelay.targetAstroId;
        TargetLPos = dFRelay.targetLPos.ToFloat3();
        TargetYaw = dFRelay.targetYaw;
        BaseTicks = dFRelay.baseTicks;

        var factory = dFRelay.hive.galaxy.astrosFactory[dFRelay.targetAstroId];
        if (factory != null)
        {
            EnemyCursor = factory.enemyCursor;
            EnemyRecyle = new int[factory.enemyRecycleCursor];
            Array.Copy(factory.enemyRecycle, EnemyRecyle, EnemyRecyle.Length);
        }
        else
        {
            EnemyCursor = -1;
            EnemyRecyle = [];
        }
    }

    public int HiveAstroId { get; set; }
    public int RelayId { get; set; }
    public int RelayNeutralizedCounter { get; set; }
    public int HiveSeed { get; set; }
    public int HiveRtseed { get; set; }
    public int TargetAstroId { get; set; }
    public Float3 TargetLPos { get; set; }
    public float TargetYaw { get; set; }
    public int BaseTicks { get; set; }
    public int EnemyCursor { get; set; }
    public int[] EnemyRecyle { get; set; }
}
