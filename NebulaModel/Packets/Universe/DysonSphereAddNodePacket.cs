using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Universe
{
    public class DysonSphereAddNodePacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public int NodeProtoId { get; set; }
        public Float3 Position { get; set; }

        public DysonSphereAddNodePacket() { }
        public DysonSphereAddNodePacket(int starIndex, int layerId, int nodeProtoId, Float3 position)
        {
            this.StarIndex = starIndex;
            this.LayerId = layerId;
            this.NodeProtoId = nodeProtoId;
            this.Position = position;
        }
    }
}
