namespace NebulaModel.Packets.Universe
{
    public class DysonSphereLoadRequest
    {
        public int StarIndex { get; set; }
        public DysonSphereLoadRequest() { }
        public DysonSphereLoadRequest(int starIndex)
        {
            this.StarIndex = starIndex;
        }
    }
}
