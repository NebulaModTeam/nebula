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

    public int PlanetId { get; }
    public int StationGId { get; }
    public int StationId { get; }
    public double TripRangeDrones { get; }
    public double TripRangeShips { get; }
    public int DeliveryDrones { get; }
    public int DeliveryShips { get; }
    public double WarperEnableDistance { get; }
    public bool WarperNecessary { get; }
    public bool IncludeOrbitCollector { get; }
    public long Energy { get; }
    public long EnergyPerTick { get; }
    public int PilerCount { get; }
    public int[] ItemId { get; }
    public int[] ItemCountMax { get; }
    public int[] ItemCount { get; }
    public int[] ItemInc { get; }
    public int[] LocalLogic { get; }
    public int[] RemoteLogic { get; }
    public int[] RemoteOrder { get; }
}
