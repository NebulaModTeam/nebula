namespace NebulaModel.Packets.Logistics
{
    public class StationUI
    {
        public bool isStorageUI { get; set; }
        // 0 == MaxChargePower
        // 1 == MaxTripDrones
        // 2 == MaxTripVessel
        // 3 == MinDeliverDrone
        // 4 == MinDeliverVessel
        // 5 == WarpDistance
        public int settingIndex { get; set; }
        public float settingValue { get; set; }
        public int stationId { get; set;}
        public int planetId { get; set; }
        public int storageIdx { get; set; }
        public int itemId { get; set; }
        public int itemCountMax { get; set; }
        public ELogisticStorage localLogic { get; set; }
        public ELogisticStorage remoteLogic { get; set; }

        public StationUI() { }
        public StationUI(int stationId, int planetId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic)
        {
            this.isStorageUI = true;
            this.stationId = stationId;
            this.planetId = planetId; // we could ommit this and instead search the galaxy when received but that would take some time
            this.storageIdx = storageIdx;
            this.itemId = itemId;
            this.itemCountMax = itemCountMax;
            this.localLogic = localLogic;
            this.remoteLogic = remoteLogic;
        }
        public StationUI(int stationId, int planetId, int settingIndex, float value)
        {
            this.stationId = stationId;
            this.planetId = planetId;
            this.isStorageUI = false;
            this.settingIndex = settingIndex;
            this.settingValue = value;
        }
    }
}
