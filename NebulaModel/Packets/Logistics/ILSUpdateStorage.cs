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

    public int GId { get; }
    public int Index { get; }
    public int Count { get; }
    public int Inc { get; }
}
