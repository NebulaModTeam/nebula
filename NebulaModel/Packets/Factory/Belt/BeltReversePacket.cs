namespace NebulaModel.Packets.Factory.Belt
{
    public class BeltReversePacket
    {
        public int BeltId { get; set; }
        public int PlanetId { get; set; }

        public BeltReversePacket() { }
        public BeltReversePacket(int beltId, int planetId)
        {
            BeltId = beltId;
            PlanetId = planetId;
        }
    }
}
