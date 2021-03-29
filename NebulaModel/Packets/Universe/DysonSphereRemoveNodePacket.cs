namespace NebulaModel.Packets.Universe
{
    public class DysonSphereRemoveNodePacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public int NodeId { get; set; }

        public DysonSphereRemoveNodePacket() { }
        public DysonSphereRemoveNodePacket(int starIndex, int layerId, int nodeId)
        {
            this.StarIndex = starIndex;
            this.LayerId = layerId;
            this.NodeId = nodeId;
        }
    }
}
