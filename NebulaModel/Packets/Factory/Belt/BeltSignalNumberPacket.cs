namespace NebulaModel.Packets.Factory.Belt
{
    public class BeltSignalNumberPacket
    {
        public int EntityId { get; set; }
        public float Number { get; set; }
        public int PlanetId { get; set; }

        public BeltSignalNumberPacket() { }
        public BeltSignalNumberPacket(int entityId, float number, int planetId)
        {
            EntityId = entityId;
            Number = number;
            PlanetId = planetId;
        }
    }
}
