namespace NebulaModel.Packets.Planet
{
    public class FactoryLoadRequest
    {
        public int PlanetID { get; set; }

        public FactoryLoadRequest() { }
        public FactoryLoadRequest(int planetID)
        {
            this.PlanetID = planetID;
        }
    }
}
