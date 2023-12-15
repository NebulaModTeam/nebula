namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereRemoveShellPacket
{
    public DysonSphereRemoveShellPacket() { }

    public DysonSphereRemoveShellPacket(int starIndex, int layerId, int shellId)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        ShellId = shellId;
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int ShellId { get; set; }
}
