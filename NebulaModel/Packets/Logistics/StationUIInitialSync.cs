namespace NebulaModel.Packets.Logistics
{
    public class StationUIInitialSync
    {
        public int stationGId { get; set; }
        public int planetId { get; set; }
        public double tripRangeDrones { get; set; }
        public double tripRangeShips { get; set; }
        public int deliveryDrones { get; set; }
        public int deliveryShips { get; set; }
        public double warpEnableDist { get; set; }
        public bool warperNecessary { get; set; }
        public bool includeOrbitCollector { get; set; }
        public long energy { get; set; }
        public long energyPerTick { get; set; }
        public int[] itemId { get; set; }
        public int[] itemCountMax { get; set; }
        public int[] itemCount { get; set; }
        public int[] localLogic { get; set; }
        public int[] remoteLogic { get; set; }
        public int[] remoteOrder { get; set; }
        public StationUIInitialSync() { }
        public StationUIInitialSync(int stationGId,
                                    int planetId,
                                    double tripRangeDrones,
                                    double tripRangeShips,
                                    int deliveryDrones,
                                    int deliveryShips,
                                    double warpEnableDist,
                                    bool warperNecessary,
                                    bool includeOrbitCollector,
                                    long energy,
                                    long energyPerTick,
                                    int[] itemId,
                                    int[] itemCountMax,
                                    int[] itemCount,
                                    int[] localLogic,
                                    int[] remoteLogic,
                                    int[] remoteOrder)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.tripRangeDrones = tripRangeDrones;
            this.tripRangeShips = tripRangeShips;
            this.deliveryDrones = deliveryDrones;
            this.deliveryShips = deliveryShips;
            this.warpEnableDist = warpEnableDist;
            this.warperNecessary = warperNecessary;
            this.includeOrbitCollector = includeOrbitCollector;
            this.energy = energy;
            this.energyPerTick = energyPerTick;

            this.itemId = itemId;
            this.itemCountMax = itemCountMax;
            this.itemCount = itemCount;
            this.localLogic = localLogic;
            this.remoteLogic = remoteLogic;
            this.remoteOrder = remoteOrder;
        }
    }
}
