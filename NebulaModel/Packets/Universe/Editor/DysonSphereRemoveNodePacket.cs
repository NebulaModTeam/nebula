namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereRemoveNodePacket
{
    public DysonSphereRemoveNodePacket() { }

    public DysonSphereRemoveNodePacket(int starIndex, int layerId, int nodeId)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        NodeId = nodeId;
    }

    public int StarIndex { get; }
    public int LayerId { get; }
    public int NodeId { get; }
}
