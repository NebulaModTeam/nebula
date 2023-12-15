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

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int NodeId { get; set; }
}
