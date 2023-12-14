namespace NebulaModel.Packets.Factory.Belt;

public class BeltSignalIconPacket
{
    public BeltSignalIconPacket() { }

    public BeltSignalIconPacket(int entityId, int signalId, int planetId)
    {
        EntityId = entityId;
        SignalId = signalId;
        PlanetId = planetId;
    }

    public int EntityId { get; }
    public int SignalId { get; }
    public int PlanetId { get; }
}
