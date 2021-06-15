namespace NebulaModel.Packets.Factory.Silo
{
    public class SiloStorageUpdatePacket
    {
        public int SiloIndex { get; set; }
        public int NewRocketsAmount { get; set; }
        public int PlanetId { get; set; }

        public SiloStorageUpdatePacket() { }
        public SiloStorageUpdatePacket(int siloIndex, int newRocketsAmount, int planetId)
        {
            SiloIndex = siloIndex;
            NewRocketsAmount = newRocketsAmount;
            PlanetId = planetId;
        }
    }
}
