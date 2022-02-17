namespace NebulaModel.Packets.Factory.Belt
{
    public class BeltSignalIconPacket
    {
        public int EntityId { get; set; }
        public int SignalId { get; set; }
        public int PlanetId { get; set; }

        public BeltSignalIconPacket() { }
        public BeltSignalIconPacket(int entityId, int signalId, int planetId)
        {
            EntityId = entityId;
            SignalId = signalId;
            PlanetId = planetId;
        }
    }
}
