namespace NebulaModel.Packets.Universe
{
    public class DysonSwarmRemoveOrbitPacket
    {
        public int StarIndex { get; set; }
        public int OrbitId { get; set; }

        public DysonSwarmRemoveOrbitPacket() { }
        public DysonSwarmRemoveOrbitPacket(int starIndex, int orbitId)
        {
            StarIndex = starIndex;
            OrbitId = orbitId;
        }
    }
}
