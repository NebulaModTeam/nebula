namespace NebulaModel.Packets.Factory.Inserter
{
    public class InserterFilterUpdatePacket
    {
        public int InserterIndex { get; set; }
        public int ItemId { get; set; }

        public InserterFilterUpdatePacket() { }

        public InserterFilterUpdatePacket(int inserterIndex, int itemId)
        {
            InserterIndex = inserterIndex;
            ItemId = itemId;
        }
    }
}
