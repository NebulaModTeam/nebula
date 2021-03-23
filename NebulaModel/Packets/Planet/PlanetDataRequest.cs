namespace NebulaModel.Packets.Planet
{
    public class PlanetDataRequest
    {
        public int[] PlanetIDs { get; set; }

        public PlanetDataRequest() { }

        public PlanetDataRequest(int[] planetIDs)
        {
            this.PlanetIDs = planetIDs;
        }
    }
}
