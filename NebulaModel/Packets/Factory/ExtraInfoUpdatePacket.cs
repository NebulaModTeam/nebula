namespace NebulaModel.Packets.Factory;

public class ExtraInfoUpdatePacket
{
    public ExtraInfoUpdatePacket() { }

    public ExtraInfoUpdatePacket(int planetId, int objId, string info)
    {
        PlanetId = planetId;
        ObjId = objId;
        Info = info;
    }

    public int PlanetId { get; set; }
    public int ObjId { get; set; }
    public string Info { get; set; }
}
