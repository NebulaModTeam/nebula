using System.Collections.Generic;
using System.Linq;

namespace NebulaModel.Packets.Planet
{
    public class PlanetDataResponse
    {
        public int[] PlanetDataIDs { get; set; }
        public byte[] PlanetDataBytes { get; set; }
        public int[] PlanetDataBytesLengths { get; set; }

        public PlanetDataResponse() { }

        public PlanetDataResponse(Dictionary<int, byte[]> planetData) 
        {
            this.PlanetDataIDs = planetData.Keys.ToArray();

            // Can't use a jagged array because LNL serializer says no, so flattening and separate offset array it is
            // TODO: Possibly register a type with the serializer instead of doing this manually
            this.PlanetDataBytes = planetData.Values.SelectMany(x => x).ToArray();
            this.PlanetDataBytesLengths = planetData.Values.Select(x => x.Length).ToArray();
        }
    }
}
