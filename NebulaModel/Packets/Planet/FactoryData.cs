namespace NebulaModel.Packets.Planet
{
    public class FactoryData
    {
        public int PlanetId { get; set; }
        public byte[] BinaryData { get; set; }
        public byte[] TerrainModData { get; set; }

        public FactoryData() { }
        public FactoryData(int id, byte[] data, byte[] terrainModData)
        {
            PlanetId = id;
            BinaryData = data;
            TerrainModData = terrainModData;
        }
    }
}
