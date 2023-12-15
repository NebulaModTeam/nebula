namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereAddFramePacket
{
    public DysonSphereAddFramePacket() { }

    public DysonSphereAddFramePacket(int starIndex, int layerId, int frameId, int protoId, int nodeAId, int nodeBId, bool euler)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        FrameId = frameId;
        ProtoId = protoId;
        NodeAId = nodeAId;
        NodeBId = nodeBId;
        Euler = euler;
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int FrameId { get; set; }
    public int ProtoId { get; set; }
    public int NodeAId { get; set; }
    public int NodeBId { get; set; }
    public bool Euler { get; set; }
}
