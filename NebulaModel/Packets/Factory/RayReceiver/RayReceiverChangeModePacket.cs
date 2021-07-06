namespace NebulaModel.Packets.Factory.RayReceiver
{
    public class RayReceiverChangeModePacket
    {
        public int GeneratorId { get; set; }
        public RayReceiverMode Mode { get; set; }
        public int PlanetId { get; set; }

        public RayReceiverChangeModePacket() { }

        public RayReceiverChangeModePacket(int generatorId, RayReceiverMode mode, int planetId)
        {
            GeneratorId = generatorId;
            Mode = mode;
            PlanetId = planetId;
        }
    }

    public enum RayReceiverMode
    {
        Electricity = 0,
        Photon = 1
    }
}
