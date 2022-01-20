namespace NebulaModel.Packets.Logistics
{
    public class ILSShipItems
    {
        public bool AddItem { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public int OrigShipIndex { get; set; }
        public int StationGID { get; set; }
        public int Inc { get; set; }

        public ILSShipItems() { }
        public ILSShipItems(bool addItem, int itemId, int itemCount, int origShipIndex, int stationGID, int inc)
        {
            AddItem = addItem;
            ItemId = itemId;
            ItemCount = itemCount;
            OrigShipIndex = origShipIndex;
            StationGID = stationGID;
            Inc = inc;
        }
    }
}
