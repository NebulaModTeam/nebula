namespace NebulaModel.Packets.Factory.Ejector
{
    public class EjectorOrbitUpdatePacket
    {
        public int EjectorIndex { get; set; }
        public int NewOrbitIndex { get; set; }
        public int FactoryIndex { get; set; }
        public EjectorOrbitUpdatePacket() { }
        public EjectorOrbitUpdatePacket(int ejectorIndex, int newOrbitIndex, int factoryIndex)
        {
            EjectorIndex = ejectorIndex;
            NewOrbitIndex = newOrbitIndex;
            FactoryIndex = factoryIndex;
        }
    }
}
