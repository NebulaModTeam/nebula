namespace NebulaModel.Packets.Factory.RayReceiver
{
    public class RayReceiverChangeModePacket
    {
        public int GeneratorId { get; set; }
        public RayReceiverMode Mode { get; set; }

        public RayReceiverChangeModePacket() { }

        public RayReceiverChangeModePacket(int generatorId, RayReceiverMode mode)
        {
            GeneratorId = generatorId;
            Mode = mode;
        }
    }

    public enum RayReceiverMode
    {
        Electricity = 0,
        Photon = 1
    }
}
