namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereRemoveLayerPacket
{
    public DysonSphereRemoveLayerPacket() { }

    public DysonSphereRemoveLayerPacket(int starIndex, int layerId)
    {
        StarIndex = starIndex;
        LayerId = layerId;
    }

    public int StarIndex { get; }
    public int LayerId { get; }
}
