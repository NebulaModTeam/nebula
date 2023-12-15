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

    public int PlanetId { get; set; }
    public int EntityId { get; set; }
    public EDispenserAddTakeEvent AddTakeEvent { get; set; }
    public int ItemId { get; set; }
    public int ItemCount { get; set; }
    public int ItemInc { get; set; }
}

public enum EDispenserAddTakeEvent
{
    None,
    ManualAdd,
    ManualTake,
    CourierAdd,
    CourierTake
}
