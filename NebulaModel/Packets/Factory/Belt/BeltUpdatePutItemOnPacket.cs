namespace NebulaModel.Packets.Factory.Belt
{
    public class BeltUpdatePutItemOnPacket
    {
        public int BeltId { get; set; }
        public int ItemId { get; set; }
        public byte ItemInc { get; set; }
        public int PlanetId { get; set; }
        public BeltUpdatePutItemOnPacket() { }
        public BeltUpdatePutItemOnPacket(int beltId, int itemId, byte itemInc, int planetId)
        {
            BeltId = beltId;
            ItemId = itemId;
            ItemInc = itemInc;
            PlanetId = planetId;
        }
    }
}
