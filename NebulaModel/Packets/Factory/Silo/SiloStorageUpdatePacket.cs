namespace NebulaModel.Packets.Factory.Silo
{
    public class SiloStorageUpdatePacket
    {
        public int SiloIndex { get; set; }
        public int NewRocketsAmount { get; set; }

        public SiloStorageUpdatePacket() { }
        public SiloStorageUpdatePacket(int siloIndex, int newRocketsAmount)
        {
            SiloIndex = siloIndex;
            NewRocketsAmount = newRocketsAmount;
        }
    }
}
