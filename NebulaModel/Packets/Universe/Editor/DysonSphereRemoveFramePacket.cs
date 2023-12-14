namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereRemoveFramePacket
{
    public DysonSphereRemoveFramePacket() { }

    public DysonSphereRemoveFramePacket(int starIndex, int layerId, int frameId)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        FrameId = frameId;
    }

    public int StarIndex { get; }
    public int LayerId { get; }
    public int FrameId { get; }
}
