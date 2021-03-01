namespace NebulaModel.Packets.Planet
{
    public class VegeMined
    {
        public int VegeID { get; set; }
        public int PlanetID { get; set; }

        public VegeMined() { }
        public VegeMined(int id, int planetID) { VegeID = id;PlanetID = planetID; }
    }
}
