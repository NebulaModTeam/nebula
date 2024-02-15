namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveRealizePacket
{
    public DFHiveRealizePacket() { }

    public DFHiveRealizePacket(int hiveAstroId)
    {
        HiveAstroId = hiveAstroId;
    }

    public int HiveAstroId { get; set; }
}
