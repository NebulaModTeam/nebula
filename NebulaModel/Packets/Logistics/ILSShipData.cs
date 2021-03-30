namespace NebulaModel.Packets.Logistics
{
    public class ILSShipData
    {
        public bool IdleToWork { get; set; }
        public int planetA { get; set; }
        public int planetB { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
        public int origArrayIndex { get; set; }

        public ILSShipData() { }
        public ILSShipData(bool IdleToWork, int planetA, int planetB, int itemId, int itemCount, int arrayIndex)
        {
            this.IdleToWork = IdleToWork;
            this.planetA = planetA;
            this.planetB = planetB;
            this.itemId = itemId;
            this.itemCount = itemCount;
            this.origArrayIndex = arrayIndex;
        }
        public ILSShipData(bool IdleToWork, int arrayIndex)
        {
            this.IdleToWork = IdleToWork;
            this.planetA = 0;
            this.planetB = 0;
            this.itemId = 0;
            this.itemCount = 0;
            this.origArrayIndex = arrayIndex;
        }
    }
}
