namespace NebulaModel.Packets.Factory.Ejector
{
    public class EjectorStorageUpdatePacket
    {
        public int EjectorIndex { get; set; }
        public int NewBulletAmount { get; set; }

        public EjectorStorageUpdatePacket() { }
        public EjectorStorageUpdatePacket(int ejectorIndex, int newBulletAmount)
        {
            EjectorIndex = ejectorIndex;
            NewBulletAmount = newBulletAmount;
        }
    }
}
