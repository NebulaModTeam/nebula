namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveClosePreviewPacket
{
    public DFHiveClosePreviewPacket() { }

    public DFHiveClosePreviewPacket(EnemyDFHiveSystem hive)
    {
        HiveAstroId = hive.hiveAstroId;
    }

    public int HiveAstroId { get; set; }
}
