namespace NebulaModel.Packets.Factory.RayReceiver
{
    public class RayReceiverChangeLensPacket
    {
        public int GeneratorId { get; set; }
        public int LensCount { get; set; }

        public RayReceiverChangeLensPacket() { }

        public RayReceiverChangeLensPacket(int generatorId, int lensCount)
        {
            GeneratorId = generatorId;
            LensCount = lensCount;
        }
    }
}
