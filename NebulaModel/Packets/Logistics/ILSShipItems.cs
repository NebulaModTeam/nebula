namespace NebulaModel.Packets.Logistics
{
    public class ILSShipItems
    {
        public bool addItem { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
        public int origShipIndex { get; set; }
        public int stationGID { get; set; }

        public ILSShipItems() { }
        public ILSShipItems(bool AddItem, int itemId, int itemCount, int origShipIndex, int stationGID)
        {
            this.addItem = AddItem;
            this.itemId = itemId;
            this.itemCount = itemCount;
            this.origShipIndex = origShipIndex;
            this.stationGID = stationGID;
        }
    }
}
