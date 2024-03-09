using System;

namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveOpenPreviewPacket
{
    public DFHiveOpenPreviewPacket() { }

    public DFHiveOpenPreviewPacket(EnemyDFHiveSystem hive, bool sendRecycle)
    {
        HiveAstroId = hive.hiveAstroId;

        if (sendRecycle)
        {
            EnemyCursor = hive.sector.enemyCursor;
            EnemyRecycle = new int[hive.sector.enemyRecycleCursor];
            Array.Copy(hive.sector.enemyRecycle, EnemyRecycle, EnemyRecycle.Length);
        }
        else
        {
            EnemyCursor = -1;
            EnemyRecycle = [];
        }
    }

    public int HiveAstroId { get; set; }
    public int EnemyCursor { get; set; }
    public int[] EnemyRecycle { get; set; }
}
