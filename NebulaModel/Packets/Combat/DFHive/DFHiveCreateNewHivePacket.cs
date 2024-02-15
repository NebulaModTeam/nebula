namespace NebulaModel.Packets.Combat.DFHive;

public class DFHiveCreateNewHivePacket
{
    public DFHiveCreateNewHivePacket() { }

    public DFHiveCreateNewHivePacket(int starId)
    {
        StarId = starId;
    }

    public int StarId { get; set; }
}
