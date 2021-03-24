using System.Collections.Generic;
using System.Linq;

namespace NebulaModel.Packets.Universe
{
    class DysonSphereResponse
    {
        public int[] StarIndexes { get; set; }
        public byte[] DysonSphereBytes { get; set; }
        public int[] DysonSphereBytesLengths { get; set; }

        public DysonSphereResponse() { }

        public DysonSphereResponse(Dictionary<int, byte[]> dysonSphereData)
        {
            this.StarIndexes = dysonSphereData.Keys.ToArray();

            // Can't use a jagged array because LNL serializer says no, so flattening and separate offset array it is
            // TODO: Possibly register a type with the serializer instead of doing this manually
            this.DysonSphereBytes = dysonSphereData.Values.SelectMany(x => x).ToArray();
            this.DysonSphereBytesLengths = dysonSphereData.Values.Select(x => x.Length).ToArray();
        }
    }
}
