namespace NebulaModel.Packets.Logistics;

public class ILSUpdateSlotData
{
    public ILSUpdateSlotData() { }

    public ILSUpdateSlotData(int planetId, int stationId, int stationGId, int index, int storageIdx)
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        Index = index;
        StorageIdx = storageIdx;
    }

    public int PlanetId { get; }
    public int StationId { get; }
    public int StationGId { get; }
    public int Index { get; }
    public int StorageIdx { get; }
}
