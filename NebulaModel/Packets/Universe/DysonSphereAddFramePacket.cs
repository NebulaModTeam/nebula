namespace NebulaModel.Packets.Universe
{
    public class DysonSphereAddFramePacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public int ProtoId { get; set; }
        public int NodeAId { get; set; }
        public int NodeBId { get; set; }
        public bool Euler { get; set; }

        public DysonSphereAddFramePacket() { }
        public DysonSphereAddFramePacket(int starIndex, int layerId, int protoId, int nodeAId, int nodeBId, bool euler)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            ProtoId = protoId;
            NodeAId = nodeAId;
            NodeBId = nodeBId;
            Euler = euler;
        }
    }
}
