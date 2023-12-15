namespace NebulaModel.Packets.Logistics;

// sent when InternalTickRemote() updates the storage count and inc values
public class ILSUpdateStorage
{
    public ILSUpdateStorage() { }

    public ILSUpdateStorage(int gid, int index, int count, int inc)
    {
        GId = gid;
        Index = index;
        Count = count;
        Inc = inc;
    }

    public int GId { get; set; }
    public int Index { get; set; }
    public int Count { get; set; }
    public int Inc { get; set; }
}
