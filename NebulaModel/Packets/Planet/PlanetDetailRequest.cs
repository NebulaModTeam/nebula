namespace NebulaModel.Packets.Planet
{
    public class PlanetDetailRequest
    {
        public int PlanetID { get; set; }

        public PlanetDetailRequest() { }

        public PlanetDetailRequest(int planetID)
        {
            PlanetID = planetID;
        }
    }
}
