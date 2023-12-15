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

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public int FrameId { get; set; }
}
