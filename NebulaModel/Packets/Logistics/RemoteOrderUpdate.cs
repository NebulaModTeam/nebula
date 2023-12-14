namespace NebulaModel.Packets.Logistics;

public class RemoteOrderUpdate
{
    public RemoteOrderUpdate() { }

    public RemoteOrderUpdate(int stationGid, int[] remoteOrder)
    {
        StationGId = stationGid;
        RemoteOrder = remoteOrder;
    }

    public int StationGId { get; set; }
    public int[] RemoteOrder { get; set; }
}
