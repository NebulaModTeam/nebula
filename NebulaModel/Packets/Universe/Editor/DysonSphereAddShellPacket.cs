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

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int ShellId { get; set; }
    public int ProtoId { get; set; }
    public int[] NodeIds { get; set; }
}
