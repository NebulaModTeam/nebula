#region

using NebulaAPI.DataStructures;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereAddNodePacket
{
    public DysonSphereAddNodePacket() { }

    public DysonSphereAddNodePacket(int starIndex, int layerId, int nodeId, int nodeProtoId, Float3 position)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        NodeId = nodeId;
        NodeProtoId = nodeProtoId;
        Position = position;
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int NodeId { get; set; }
    public int NodeProtoId { get; set; }
    public Float3 Position { get; set; }
}
