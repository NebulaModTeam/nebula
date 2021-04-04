namespace NebulaModel.Packets.Logistics
{
    public class ILSRemoteOrderData
    {
        public int stationGID { get; set; }
        public int storageIndex { get; set; }
        public int remoteOrder { get; set; }

        public ILSRemoteOrderData() { }
        public ILSRemoteOrderData(int stationGID, int storageIndex, int remoteOrder)
        {
            this.stationGID = stationGID;
            this.storageIndex = storageIndex;
            this.remoteOrder = remoteOrder;
        }
    }
}
