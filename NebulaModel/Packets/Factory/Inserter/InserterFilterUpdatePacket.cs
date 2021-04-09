namespace NebulaModel.Packets.Factory.Inserter
{
    public class InserterFilterUpdatePacket
    {
        public int InserterIndex { get; set; }
        public int ItemId { get; set; }
        public int FactoryIndex { get; set; }

        public InserterFilterUpdatePacket() { }

        public InserterFilterUpdatePacket(int inserterIndex, int itemId, int factoryIndex)
        {
            InserterIndex = inserterIndex;
            ItemId = itemId;
            FactoryIndex = factoryIndex;
        }
    }
}
