using System;

namespace NebulaModel.Packets.Logistics
{
    public class StationUIInitialSync
    {
        public int stationGId { get; set; }
        public double tripRangeDrones { get; set; }
        public double tripRangeShips { get; set; }
        public int deliveryDrones { get; set; }
        public int deliveryShips { get; set; }
        public double warpEnableDist { get; set; }
        public bool warperNecessary { get; set; }
        public bool includeOrbitCollector { get; set; }
        public int[] itemId { get; set; }
        public int[] itemCountMax { get; set; }
        public int[] localLogic { get; set; }
        public int[] remoteLogic { get; set; }
        public StationUIInitialSync() { }
        public StationUIInitialSync(int stationGId,
                                    double tripRangeDrones,
                                    double tripRangeShips,
                                    int deliveryDrones,
                                    int deliveryShips,
                                    double warpEnableDist,
                                    bool warperNecessary,
                                    bool includeOrbitCollector,
                                    int[] itemId,
                                    int[] itemCountMax,
                                    int[] localLogic,
                                    int[] remoteLogic)
        {
            this.itemId = new int[itemId.Length];
            this.itemCountMax = new int[itemCountMax.Length];
            this.localLogic = new int[localLogic.Length];
            this.remoteLogic = new int[remoteLogic.Length];

            this.stationGId = stationGId;
            this.tripRangeDrones = tripRangeDrones;
            this.tripRangeShips = tripRangeShips;
            this.deliveryDrones = deliveryDrones;
            this.deliveryShips = deliveryShips;
            this.warpEnableDist = warpEnableDist;
            this.warperNecessary = warperNecessary;
            this.includeOrbitCollector = includeOrbitCollector;

            Array.Copy(itemId, this.itemId, itemId.Length);
            Array.Copy(itemCountMax, this.itemCountMax, itemCountMax.Length);
            Array.Copy(localLogic, this.localLogic, localLogic.Length);
            Array.Copy(remoteLogic, this.remoteLogic, remoteLogic.Length);
        }
    }
}
