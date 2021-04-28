namespace NebulaModel.Packets.Logistics
{
    public class StationUI
    {
        public enum UIsettings
        {
            MaxChargePower,
            MaxTripDrones,
            MaxTripVessel,
            MinDeliverDrone,
            MinDeliverVessel,
            WarpDistance,
            warperNeeded,
            includeCollectors,
            setDroneCount,
            setShipCount,
            setWarperCount,
            addOrRemoveItemFromStorageReq,
            addOrRemoveItemFromStorageResp
        }
        public bool isStorageUI { get; set; }
        public StationUI.UIsettings settingIndex { get; set; }
        public float settingValue { get; set; }
        public int stationGId { get; set;} // NOTE: this can also be the id when handling a PLS
        public int planetId { get; set; }
        public int storageIdx { get; set; }
        public int itemId { get; set; }
        public int itemCountMax { get; set; }
        public ELogisticStorage localLogic { get; set; }
        public ELogisticStorage remoteLogic { get; set; }
        public bool shouldMimick { get; set; }
        public bool isStellar { get; set; }

        public StationUI() { }
        public StationUI(int stationGId, int planetId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, bool isStellar)
        {
            this.isStorageUI = true;
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.storageIdx = storageIdx;
            this.itemId = itemId;
            this.itemCountMax = itemCountMax;
            this.localLogic = localLogic;
            this.remoteLogic = remoteLogic;
            this.shouldMimick = false;
            this.isStellar = isStellar;
        }
        public StationUI(int stationGId, int planetId, StationUI.UIsettings settingIndex, float value)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.isStorageUI = false;
            this.settingIndex = settingIndex;
            this.settingValue = value;
            this.isStellar = true;
        }
        public StationUI(int stationGId, int planetId, int storageIdx, StationUI.UIsettings settingIndex, int itemId, int settingValue, bool isStellar)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.settingIndex = settingIndex;
            this.itemId = itemId;
            this.settingValue = settingValue;
            this.storageIdx = storageIdx;
            this.isStellar = isStellar;
        }
    }
}
