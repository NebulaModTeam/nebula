namespace NebulaModel.Packets.Factory.Ejector
{
    public class EjectorStorageUpdatePacket
    {
        public int EjectorIndex { get; set; }
        public int NewBulletAmount { get; set; }
        public int PlanetId { get; set; }

        public EjectorStorageUpdatePacket() { }
        public EjectorStorageUpdatePacket(int ejectorIndex, int newBulletAmount, int planetId)
        {
            EjectorIndex = ejectorIndex;
            NewBulletAmount = newBulletAmount;
            PlanetId = planetId;
        }
    }
}
