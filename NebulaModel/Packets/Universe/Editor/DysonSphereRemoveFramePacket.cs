namespace NebulaModel.Packets.Universe
{
    public class DysonSphereRemoveFramePacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public int FrameId { get; set; }

        public DysonSphereRemoveFramePacket() { }
        public DysonSphereRemoveFramePacket(int starIndex, int layerId, int frameId)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            FrameId = frameId;
        }
    }
}
