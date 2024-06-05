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
        DroneAutoReplenish,
        ShipAutoReplenish,
        RemoteGroupMask,
        RoutePriority
    }

    public StationUI() { }

    public StationUI(int planetId, int stationId, int stationGId, EUISettings settingIndex, double value,
        bool warperShouldTakeFromStorage = false)
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        SettingIndex = settingIndex;
        SettingValue = value;
        WarperShouldTakeFromStorage = warperShouldTakeFromStorage;
    }

    public int PlanetId { get; set; }
    public int StationId { get; set; }
    public int StationGId { get; set; }
    public EUISettings SettingIndex { get; set; }
    public double SettingValue { get; set; }
    public bool WarperShouldTakeFromStorage { get; set; }
    public bool ShouldRefund { get; set; }
}
