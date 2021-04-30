/*
 * see request for info
 */
namespace NebulaModel.Packets.Logistics
{
    public class ILSArriveStarPlanetResponse
    {
        public int[] stationGId { get; set; }
        public int[] planetId { get; set; }
        public int planet { get; set; }
        public int[] storageLength { get; set; }
        public int[] storageIdx { get; set; }
        public int[] itemId { get; set; }
        public int[] count { get; set; }
        public int[] localOrder { get; set; }
        public int[] remoteOrder { get; set; }
        public int[] max { get; set; }
        public int[] localLogic { get; set; }
        public int[] remoteLogic { get; set; }
        public ILSArriveStarPlanetResponse() { }
        public ILSArriveStarPlanetResponse(int[] stationGId,
                                        int[] planetId,
                                        int planet,
                                        int[] storageLength,
                                        int[] storageIdx,
                                        int[] itemId,
                                        int[] count,
                                        int[] localOrder,
                                        int[] remoteOrder,
                                        int[] max,
                                        int[] localLogic,
                                        int[] remoteLogic)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.planet = planet;
            this.storageLength = storageLength;
            this.storageIdx = storageIdx;
            this.itemId = itemId;
            this.count = count;
            this.localOrder = localOrder;
            this.remoteOrder = remoteOrder;
            this.max = max;
            this.localLogic = localLogic;
            this.remoteLogic = remoteLogic;
        }
    }
}
