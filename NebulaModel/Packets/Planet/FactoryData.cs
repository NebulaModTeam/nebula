namespace NebulaModel.Packets.Planet;

public class FactoryData
{
    public FactoryData() { }

    public FactoryData(int id, byte[] data, byte[] terrainModData)
    {
        PlanetId = id;
        BinaryData = data;
        TerrainModData = terrainModData;
    }

    public int PlanetId { get; }
    public byte[] BinaryData { get; }
    public byte[] TerrainModData { get; }
}
