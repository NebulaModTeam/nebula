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
            PilerCount,
            MaxMiningSpeed,
            NameInput
        }

        public int PlanetId { get; set; }
        public int StationId { get; set; }
        public int StationGId { get; set; }
        public StationUI.EUISettings SettingIndex { get; set; }
        public float SettingValue { get; set; }
        public string SettingString { get; set; }
        public bool WarperShouldTakeFromStorage { get; set; }
        public bool ShouldRefund { get; set; }

        public StationUI() { }
        public StationUI(int planetId, int stationId, int stationGId, StationUI.EUISettings settingIndex, float value, bool warperShouldTakeFromStorage = false)
        {
            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            SettingIndex = settingIndex;
            SettingValue = value;
            WarperShouldTakeFromStorage = warperShouldTakeFromStorage;
        }
        public StationUI(int planetId, int stationId, int stationGId, StationUI.EUISettings settingIndex, string settingString)
        {
            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            SettingIndex = settingIndex;
            SettingString = settingString;
        }
    }
}
