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

    public int PlanetId { get; set; }
    public int StationId { get; set; }
    public int StationGId { get; set; }
    public int Index { get; set; }
    public int StorageIdx { get; set; }
}
