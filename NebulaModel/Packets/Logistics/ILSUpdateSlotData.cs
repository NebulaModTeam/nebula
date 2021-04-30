namespace NebulaModel.Packets.Logistics
{
    public class ILSUpdateSlotData
    {
        public int StationGId { get; set; }
        public int PlanetId { get; set; }
        public int Index { get; set; }
        public int StorageIdx { get; set; }
        public ILSUpdateSlotData() { }
        public ILSUpdateSlotData(int stationGId,
                                    int planetId,
                                    int index,
                                    int storageIdx)
        {
            this.StationGId = stationGId;
            this.PlanetId = planetId;
            this.Index = index;
            this.StorageIdx = storageIdx;
        }
    }
}
