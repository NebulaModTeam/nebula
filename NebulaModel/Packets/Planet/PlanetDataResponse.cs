namespace NebulaModel.Packets.Planet
{
    public class PlanetDataResponse
    {
        public int PlanetDataID { get; set; }
        public byte[] PlanetDataByte { get; set; }

        public PlanetDataResponse() { }

        public PlanetDataResponse(int planetId, byte[] planetData)
        {
            PlanetDataID = planetId;
            PlanetDataByte = planetData;
        }
    }
}
