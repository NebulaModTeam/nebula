namespace NebulaModel.Packets.Logistics
{
    public class RemoteOrderUpdate
    {
        public int StationGId { get; set; }
        public int[] RemoteOrder { get; set; }

        public RemoteOrderUpdate() { }
        public RemoteOrderUpdate(int stationGid, int[] remoteOrder)
        {
            StationGId = stationGid;
            RemoteOrder = remoteOrder;
        }
    }
}
