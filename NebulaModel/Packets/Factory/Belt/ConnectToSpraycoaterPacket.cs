namespace NebulaModel.Packets.Factory.Belt
{
    public class ConnectToSpraycoaterPacket
    {
        public int SpraycoaterId { get; set; }
        public int CargoBeltId { get; set; }
        public int IncBeltId { get; set; }
        public int PlanetId { get; set; }

        public ConnectToSpraycoaterPacket() { }
        public ConnectToSpraycoaterPacket(int spraycoaterId, int cargoBeltId, int incBeltId, int planetId)
        {
            SpraycoaterId = spraycoaterId;
            CargoBeltId = cargoBeltId;
            IncBeltId = incBeltId;
            PlanetId = planetId;
        }
    }
}
