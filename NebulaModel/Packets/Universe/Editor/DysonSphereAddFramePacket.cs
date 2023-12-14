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

    public int StarIndex { get; }
    public int LayerId { get; }
    public int FrameId { get; }
    public int ProtoId { get; }
    public int NodeAId { get; }
    public int NodeBId { get; }
    public bool Euler { get; }
}
