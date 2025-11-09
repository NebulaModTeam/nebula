using System;
using System.Collections.Generic;

namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPStationNameSearchPacket
{
    public LCPStationNameSearchPacket() { }

    public string SearchString { get; set; }
    public bool IsExact { get; set; }
    public int LocalPlanetId { get; set; }
    public int[] ResultGids { get; set; }
    public string[] ResultNames { get; set; }
}
