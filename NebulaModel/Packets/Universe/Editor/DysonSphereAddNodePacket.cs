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

    public int StarIndex { get; }
    public int LayerId { get; }
    public int NodeId { get; }
    public int NodeProtoId { get; }
    public Float3 Position { get; }
}
