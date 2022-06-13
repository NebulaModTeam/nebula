namespace NebulaModel.Packets.Planet
{
    public class VegeAddPacket
    {
        public int PlanetId { get; set; }
        public bool IsVein { get; set; }
        public byte[] Data { get; set; }

        public VegeAddPacket() { }
        public VegeAddPacket(int planetId, bool isVein, byte[] data)
        {
            PlanetId = planetId;
            IsVein = isVein;
            Data = data;
        }
    }
}
