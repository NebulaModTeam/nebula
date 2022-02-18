﻿/*
 * see request for info
 */
namespace NebulaModel.Packets.Logistics
{
    public class ILSArriveStarPlanetResponse
    {
        public int[] StationGId { get; set; }
        public int[] StationPId { get; set; }
        
        public int[] StationMaxShips { get; set; }
        public int[] StorageLength { get; set; }
        public int[] StorageIdx { get; set; }
        public int[] ItemId { get; set; }
        public int[] Count { get; set; }
        public int[] Inc { get; set; }
        public ILSArriveStarPlanetResponse() { }
        public ILSArriveStarPlanetResponse(int[] stationGId,
                                        int[] planetId,
                                        int[] stationMaxShips,
                                        int[] storageLength,
                                        int[] storageIdx,
                                        int[] itemId,
                                        int[] count,
                                        int[] inc)
        {
            StationGId = stationGId;
            StationPId = planetId;
            StationMaxShips = stationMaxShips;
            StorageLength = storageLength;
            StorageIdx = storageIdx;
            ItemId = itemId;
            Count = count;
            Inc = inc;
        }
    }
}
