namespace NebulaModel.Packets.Logistics;

public class ILSArriveStarPlanetResponse
{
    public ILSArriveStarPlanetResponse() { }

    public ILSArriveStarPlanetResponse(int[] stationGId,
        int[] planetId,
        int[] stationMaxShips,
        int[] storageLength,
        int[] storageIdx,
        int[] slotLength,
        int[] itemId,
        int[] count,
        int[] inc)
    {
        StationGId = stationGId;
        StationPId = planetId;
        StationMaxShips = stationMaxShips;
        StorageLength = storageLength;
        StorageIdx = storageIdx;
        SlotLength = slotLength;
        ItemId = itemId;
        Count = count;
        Inc = inc;
    }

    public int[] StationGId { get; }
    public int[] StationPId { get; }

    public int[] StationMaxShips { get; }
    public int[] StorageLength { get; }
    public int[] SlotLength { get; }
    public int[] StorageIdx { get; }
    public int[] ItemId { get; }
    public int[] Count { get; }
    public int[] Inc { get; }
}
