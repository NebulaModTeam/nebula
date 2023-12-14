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

    public int StarIndex { get; }
    public int OrbitId { get; }
    public SwarmRemoveOrbitEvent Event { get; }
}

public enum SwarmRemoveOrbitEvent
{
    Remove,
    Disable,
    Enable,
    RemoveSails
}
