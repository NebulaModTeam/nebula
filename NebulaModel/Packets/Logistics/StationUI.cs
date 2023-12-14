namespace NebulaModel.Packets.Logistics;

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
        NameInput,
        DroneAutoReplenish,
        ShipAutoReplenish
    }

    public StationUI() { }

    public StationUI(int planetId, int stationId, int stationGId, EUISettings settingIndex, float value,
        bool warperShouldTakeFromStorage = false)
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        SettingIndex = settingIndex;
        SettingValue = value;
        WarperShouldTakeFromStorage = warperShouldTakeFromStorage;
    }

    public StationUI(int planetId, int stationId, int stationGId, EUISettings settingIndex, string settingString)
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        SettingIndex = settingIndex;
        SettingString = settingString;
    }

    public int PlanetId { get; }
    public int StationId { get; }
    public int StationGId { get; }
    public EUISettings SettingIndex { get; set; }
    public float SettingValue { get; set; }
    public string SettingString { get; }
    public bool WarperShouldTakeFromStorage { get; }
    public bool ShouldRefund { get; set; }
}
