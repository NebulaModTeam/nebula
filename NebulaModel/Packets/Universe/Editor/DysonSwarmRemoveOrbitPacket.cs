namespace NebulaModel.Packets.Universe
{
    public class DysonSwarmRemoveOrbitPacket
    {
        public int StarIndex { get; set; }
        public int OrbitId { get; set; }
        public SwarmRemoveOrbitEvent Event { get; set; }

        public DysonSwarmRemoveOrbitPacket() { }
        public DysonSwarmRemoveOrbitPacket(int starIndex, int orbitId, SwarmRemoveOrbitEvent removeEvent)
        {
            StarIndex = starIndex;
            OrbitId = orbitId;
            Event = removeEvent;
        }
    }

    public enum SwarmRemoveOrbitEvent
    {
        Remove,
        Disable,
        Enable,
        RemoveSails
    }
}
