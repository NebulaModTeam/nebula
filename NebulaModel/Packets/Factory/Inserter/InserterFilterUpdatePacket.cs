namespace NebulaModel.Packets.Factory.Inserter
{
    public class InserterFilterUpdatePacket
    {
        public int InserterIndex { get; set; }
        public int ItemId { get; set; }
        public int PlanetId { get; set; }

        public InserterFilterUpdatePacket() { }

        public InserterFilterUpdatePacket(int inserterIndex, int itemId, int planetId)
        {
            InserterIndex = inserterIndex;
            ItemId = itemId;
            PlanetId = planetId;
        }
    }
}
