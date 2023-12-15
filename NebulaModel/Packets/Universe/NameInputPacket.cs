#region

using System.Collections.Generic;
using NebulaAPI;

#endregion

namespace NebulaModel.Packets.Universe;

// Packet for name input for Planets and Stars
public class NameInputPacket
{
    public NameInputPacket() { }

    public NameInputPacket(string name, int starId, int planetId)
    {
        Names = new[] { name };
        StarIds = new[] { starId };
        PlanetIds = new[] { planetId };
    }

    public NameInputPacket(in GalaxyData galaxy)
    {
        var names = new List<string>();
        var starIds = new List<int>();
        var planetIds = new List<int>();

        foreach (var s in galaxy.stars)
        {
            if (!string.IsNullOrEmpty(s.overrideName))
            {
                names.Add(s.overrideName);
                starIds.Add(s.id);
                planetIds.Add(NebulaModAPI.PLANET_NONE);
            }
            foreach (var p in s.planets)
            {
                if (string.IsNullOrEmpty(p.overrideName))
                {
                    continue;
                }
                names.Add(p.overrideName);
                starIds.Add(NebulaModAPI.STAR_NONE);
                planetIds.Add(p.id);
            }
        }

        Names = names.ToArray();
        StarIds = starIds.ToArray();
        PlanetIds = planetIds.ToArray();
    }

    public string[] Names { get; set; }
    public int[] PlanetIds { get; set; }
    public int[] StarIds { get; set; }
}
