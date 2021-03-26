namespace NebulaModel.Packets.Universe
{
    public class DysonSwarmRemoveOrbitPacket
    {
        public int StarIndex { get; set; }
        public int OrbitId { get; set; }

        public DysonSwarmRemoveOrbitPacket() { }
        public DysonSwarmRemoveOrbitPacket(int starIndex, int orbitId)
        {
            this.StarIndex = starIndex;
            this.OrbitId = orbitId;
        }
    }
}
