namespace NebulaModel.Packets.Planet
{
    public class FactoryData
    {
        public int PlanetId { get; set; }
        public byte[] BinaryData { get; set; }

        public FactoryData() { }
        public FactoryData(int id, byte[] data)
        {
            this.PlanetId = id;
            this.BinaryData = data;
        }
    }
}
