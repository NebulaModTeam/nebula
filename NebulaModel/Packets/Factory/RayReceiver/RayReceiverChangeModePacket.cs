namespace NebulaModel.Packets.Factory.RayReceiver
{
    public class RayReceiverChangeModePacket
    {
        public int GeneratorId { get; set; }
        public RayReceiverMode Mode { get; set; }
        public int FactoryIndex { get; set; }

        public RayReceiverChangeModePacket() { }

        public RayReceiverChangeModePacket(int generatorId, RayReceiverMode mode, int factoryIndex)
        {
            GeneratorId = generatorId;
            Mode = mode;
            FactoryIndex = factoryIndex;
        }
    }

    public enum RayReceiverMode
    {
        Electricity = 0,
        Photon = 1
    }
}
