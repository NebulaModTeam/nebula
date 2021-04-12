namespace NebulaModel.Packets.Factory.RayReceiver
{
    public class RayReceiverChangeLensPacket
    {
        public int GeneratorId { get; set; }
        public int LensCount { get; set; }
        public int FactoryIndex { get; set; }

        public RayReceiverChangeLensPacket() { }

        public RayReceiverChangeLensPacket(int generatorId, int lensCount, int factoryIndex)
        {
            GeneratorId = generatorId;
            LensCount = lensCount;
            FactoryIndex = factoryIndex;
        }
    }
}
