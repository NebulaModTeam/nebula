namespace NebulaModel.Packets.Logistics
{
    public class ILSUpdateSlotData
    {
        public int PlanetId { get; set; }
        public int StationId { get; set; }
        public int StationGId { get; set; }
        public int Index { get; set; }
        public int StorageIdx { get; set; }
        public ILSUpdateSlotData() { }
        public ILSUpdateSlotData(int planetId, int stationId, int stationGId, int index, int storageIdx)
        {
            StationGId = stationGId;
            PlanetId = planetId;
            StationId = stationId;
            Index = index;
            StorageIdx = storageIdx;
        }
    }
}
