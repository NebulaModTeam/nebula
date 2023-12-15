namespace NebulaModel.Packets.Logistics;

public class StationUIInitialSync
{
    public StationUIInitialSync() { }

    public StationUIInitialSync(
        int planetId,
        int stationId,
        int stationGId,
        double tripRangeDrones,
        double tripRangeShips,
        int deliveryDrones,
        int deliveryShips,
        double warperEnableDistance,
        bool warperNecessary,
        bool includeOrbitCollector,
        long energy,
        long energyPerTick,
        int pilerCount,
        int[] itemId,
        int[] itemCountMax,
        int[] itemCount,
        int[] itemInc,
        int[] localLogic,
        int[] remoteLogic,
        int[] remoteOrder
    )
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        TripRangeDrones = tripRangeDrones;
        TripRangeShips = tripRangeShips;
        DeliveryDrones = deliveryDrones;
        DeliveryShips = deliveryShips;
        WarperEnableDistance = warperEnableDistance;
        WarperNecessary = warperNecessary;
        IncludeOrbitCollector = includeOrbitCollector;
        Energy = energy;
        EnergyPerTick = energyPerTick;
        PilerCount = pilerCount;

        ItemId = itemId;
        ItemCountMax = itemCountMax;
        ItemCount = itemCount;
        ItemInc = itemInc;
        LocalLogic = localLogic;
        RemoteLogic = remoteLogic;
        RemoteOrder = remoteOrder;
    }

    public int PlanetId { get; set; }
    public int StationGId { get; set; }
    public int StationId { get; set; }
    public double TripRangeDrones { get; set; }
    public double TripRangeShips { get; set; }
    public int DeliveryDrones { get; set; }
    public int DeliveryShips { get; set; }
    public double WarperEnableDistance { get; set; }
    public bool WarperNecessary { get; set; }
    public bool IncludeOrbitCollector { get; set; }
    public long Energy { get; set; }
    public long EnergyPerTick { get; set; }
    public int PilerCount { get; set; }
    public int[] ItemId { get; set; }
    public int[] ItemCountMax { get; set; }
    public int[] ItemCount { get; set; }
    public int[] ItemInc { get; set; }
    public int[] LocalLogic { get; set; }
    public int[] RemoteLogic { get; set; }
    public int[] RemoteOrder { get; set; }
}
