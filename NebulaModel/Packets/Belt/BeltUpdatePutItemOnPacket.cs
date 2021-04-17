namespace NebulaModel.Packets.Belt
{
    public class BeltUpdatePutItemOnPacket
    {
        public int BeltId { get; set; }
        public int ItemId { get; set; }
        public int FactoryIndex { get; set; }
        public BeltUpdatePutItemOnPacket() { }
        public BeltUpdatePutItemOnPacket(int beltId, int itemId, int factoryIndex)
        {
            BeltId = beltId;
            ItemId = itemId;
            FactoryIndex = factoryIndex;
        }
    }
}
