namespace NebulaModel.Packets.Logistics;

public class DispenserAddTakePacket
{
    public DispenserAddTakePacket() { }

    public DispenserAddTakePacket(int planetId, int entityId, EDispenserAddTakeEvent addTakeEvent, int itemId, int itemCount,
        int itemInc)
    {
        PlanetId = planetId;
        EntityId = entityId;
        AddTakeEvent = addTakeEvent;
        ItemId = itemId;
        ItemCount = itemCount;
        ItemInc = itemInc;
    }

    public int PlanetId { get; }
    public int EntityId { get; }
    public EDispenserAddTakeEvent AddTakeEvent { get; }
    public int ItemId { get; }
    public int ItemCount { get; }
    public int ItemInc { get; }
}

public enum EDispenserAddTakeEvent
{
    None,
    ManualAdd,
    ManualTake,
    CourierAdd,
    CourierTake
}
