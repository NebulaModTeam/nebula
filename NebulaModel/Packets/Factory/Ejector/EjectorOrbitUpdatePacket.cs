namespace NebulaModel.Packets.Factory.Ejector
{
    public class EjectorOrbitUpdatePacket
    {
        public int EjectorIndex { get; set; }
        public int NewOrbitIndex { get; set; }

        public EjectorOrbitUpdatePacket() { }
        public EjectorOrbitUpdatePacket(int ejectorIndex, int newOrbitIndex)
        {
            EjectorIndex = ejectorIndex;
            NewOrbitIndex = newOrbitIndex;
        }
    }
}
