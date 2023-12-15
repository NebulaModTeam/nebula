namespace NebulaModel.Packets.Universe.Editor;

public class DysonSwarmRemoveOrbitPacket
{
    public DysonSwarmRemoveOrbitPacket() { }

    public DysonSwarmRemoveOrbitPacket(int starIndex, int orbitId, SwarmRemoveOrbitEvent removeEvent)
    {
        StarIndex = starIndex;
        OrbitId = orbitId;
        Event = removeEvent;
    }

    public int StarIndex { get; set; }
    public int OrbitId { get; set; }
    public SwarmRemoveOrbitEvent Event { get; set; }
}

public enum SwarmRemoveOrbitEvent
{
    Remove,
    Disable,
    Enable,
    RemoveSails
}
