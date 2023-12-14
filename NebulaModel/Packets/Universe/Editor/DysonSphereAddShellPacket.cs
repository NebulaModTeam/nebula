#region

using System.Collections.Generic;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereAddShellPacket
{
    public DysonSphereAddShellPacket() { }

    public DysonSphereAddShellPacket(int starIndex, int layerId, int shellId, int protoId, List<int> nodeIds)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        ShellId = shellId;
        ProtoId = protoId;
        NodeIds = nodeIds.ToArray();
    }

    public int StarIndex { get; }
    public int LayerId { get; }
    public int ShellId { get; }
    public int ProtoId { get; }
    public int[] NodeIds { get; }
}
