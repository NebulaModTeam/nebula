using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NebulaModel.Packets
{
    public class VegeMined
    {
        public int VegeID { get; set; }
        public int PlanetID { get; set; }

        public VegeMined(int id, int planetID) { VegeID = id;PlanetID = planetID; }
        public VegeMined() { }
    }
}
