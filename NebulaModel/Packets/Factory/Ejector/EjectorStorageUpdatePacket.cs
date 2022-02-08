namespace NebulaModel.Packets.Factory.Ejector
{
    public class EjectorStorageUpdatePacket
    {
        public int EjectorIndex { get; set; }
        public int ItemCount { get; set; }
        public int ItemInc { get; set; }
        public int PlanetId { get; set; }

        public EjectorStorageUpdatePacket() { }
        public EjectorStorageUpdatePacket(int ejectorIndex, int itemCount, int itemInc, int planetId)
        {
            EjectorIndex = ejectorIndex;
            ItemCount = itemCount;
            ItemInc = itemInc;
            PlanetId = planetId;
        }
    }
}
