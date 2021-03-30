namespace NebulaModel.Packets.Logistics
{
    public class StationUI
    {
        public int stationId { get; set;}
        public int storageIdx { get; set; }
        public int itemId { get; set; }
        public int itemCountMax { get; set; }
        public ELogisticStorage localLogic { get; set; }
        public ELogisticStorage remoteLogic { get; set; }

        public StationUI() { }
        public StationUI(int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic)
        {
            this.stationId = stationId;
            this.storageIdx = storageIdx;
            this.itemId = itemId;
            this.itemCountMax = itemCountMax;
            this.localLogic = localLogic;
            this.remoteLogic = remoteLogic;
        }
    }
}
