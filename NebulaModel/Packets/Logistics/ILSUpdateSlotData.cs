namespace NebulaModel.Packets.Logistics
{
    public class ILSUpdateSlotData
    {
        public int stationGId { get; set; }
        public int planetId { get; set; }
        public int index { get; set; }
        public int storageIdx { get; set; }
        public ILSUpdateSlotData() { }
        public ILSUpdateSlotData(int stationGId,
                                    int planetId,
                                    int index,
                                    int storageIdx)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.index = index;
            this.storageIdx = storageIdx;
        }
    }
}
