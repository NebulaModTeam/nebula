using System;
using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSLaunchLancerAssaultPacket
{
    public DFSLaunchLancerAssaultPacket() { }

    public DFSLaunchLancerAssaultPacket(in EnemyDFHiveSystem hive, EAggressiveLevel aggressiveLevel,
        in Vector3 tarPos, in Vector3 maxHatredPos, int targetAstroId, int unitCount0, int unitThreat)
    {
        HiveAstroId = hive.hiveAstroId;
        EvolveThreat = hive.evolve.threat;
        AggressiveLevel = (short)aggressiveLevel;
        TarPos = tarPos.ToFloat3();
        MaxHatredPos = maxHatredPos.ToFloat3();
        TargetAstroId = targetAstroId;
        UnitCount0 = unitCount0;
        UnitThreat = unitThreat;

        EnemyCursor = hive.sector.enemyCursor;
        EnemyRecycle = new int[hive.sector.enemyRecycleCursor];
        Array.Copy(hive.sector.enemyRecycle, EnemyRecycle, EnemyRecycle.Length);
    }

    public int HiveAstroId { get; set; }
    public int EvolveThreat { get; set; }
    public short AggressiveLevel { get; set; }
    public Float3 TarPos { get; set; }
    public Float3 MaxHatredPos { get; set; }
    public int TargetAstroId { get; set; }
    public int UnitCount0 { get; set; }
    public int UnitThreat { get; set; }
    public int EnemyCursor { get; set; }
    public int[] EnemyRecycle { get; set; }
}
