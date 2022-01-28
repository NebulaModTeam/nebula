namespace NebulaModel.Packets.Logistics
{
    public class StationUI
    {
        public enum EUISettings
        {
            None,
            MaxChargePower,
            MaxTripDrones,
            MaxTripVessel,
            MinDeliverDrone,
            MinDeliverVessel,
            WarpDistance,
            WarperNeeded,
            IncludeCollectors,
            SetDroneCount,
            SetShipCount,
            SetWarperCount,
            AddOrRemoveItemFromStorage,
            PilerCount,
            MaxMiningSpeed
        }

        public int PlanetId { get; set; }
        public int StationId { get; set; }
        public int StationGId { get; set; }
        public bool IsStorageUI { get; set; }
        public StationUI.EUISettings SettingIndex { get; set; }
        public float SettingValue { get; set; }
        public float SettingValue2 { get; set; }
        public int StorageIdx { get; set; }
        public int ItemId { get; set; }
        public int ItemCountMax { get; set; }
        public ELogisticStorage LocalLogic { get; set; }
        public ELogisticStorage RemoteLogic { get; set; }
        public bool ShouldMimic { get; set; }
        public bool WarperShouldTakeFromStorage { get; set; }

        public StationUI() { }
        public StationUI(int planetId, int stationId, int stationGId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic)
        {
            IsStorageUI = true;
            ShouldMimic = false;
            WarperShouldTakeFromStorage = false;

            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            StorageIdx = storageIdx;
            ItemId = itemId;
            ItemCountMax = itemCountMax;
            LocalLogic = localLogic;
            RemoteLogic = remoteLogic;
        }
        public StationUI(int planetId, int stationId, int stationGId, StationUI.EUISettings settingIndex, float value, bool warperShouldTakeFromStorage = false)
        {
            IsStorageUI = false;

            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            SettingIndex = settingIndex;
            SettingValue = value;
            WarperShouldTakeFromStorage = warperShouldTakeFromStorage;
        }
        public StationUI(int planetId, int stationId, int stationGId, int storageIdx, StationUI.EUISettings settingIndex, int itemId, int count, int inc)
        {
            WarperShouldTakeFromStorage = false;

            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            StorageIdx = storageIdx;
            SettingIndex = settingIndex;
            ItemId = itemId;
            SettingValue = count;
            SettingValue2 = inc;
        }
    }
}
