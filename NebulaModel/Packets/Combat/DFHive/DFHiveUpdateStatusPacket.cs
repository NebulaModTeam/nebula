namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveUpdateStatusPacket
{
    public DFHiveUpdateStatusPacket() { }

    public DFHiveUpdateStatusPacket(in EnemyDFHiveSystem hive)
    {
        HiveAstroId = hive.hiveAstroId;
        ref var evolveData = ref hive.evolve;
        Threat = evolveData.threat;
        Level = evolveData.level;
        Expl = evolveData.expl;
        Expf = evolveData.expf;
    }

    public void Record(in EnemyDFHiveSystem hive)
    {
        ref var evolveData = ref hive.evolve;
        Threat = evolveData.threat;
        Level = evolveData.level;
        Expl = evolveData.expl;
        Expf = evolveData.expf;
    }

    public int HiveAstroId { get; set; }
    public int Threat { get; set; }
    public int Level { get; set; }
    public int Expl { get; set; }
    public int Expf { get; set; }
}
