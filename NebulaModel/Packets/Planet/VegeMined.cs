namespace NebulaModel.Packets.Planet
{
    public class VegeMined
    {
        public int MiningID { get; set; }
        // if it is not a vegetable it is a vein
        public bool isVegetable { get; set; }
        public int PlanetID { get; set; }

        public VegeMined() { }
        public VegeMined(int id, bool isVege, int planetID) { MiningID = id; isVegetable = isVege; PlanetID = planetID; }
    }
}
