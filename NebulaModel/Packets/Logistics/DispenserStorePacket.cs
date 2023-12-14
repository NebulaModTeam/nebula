namespace NebulaModel.Packets.Logistics;

public class DispenserStorePacket
{
    public DispenserStorePacket() { }

    public DispenserStorePacket(int planetId, in DispenserComponent dispenser)
    {
        PlanetId = planetId;
        DispenserId = dispenser.id;
        HoldupItemCount = dispenser.holdupItemCount;
        ItemIds = new int[HoldupItemCount];
        Counts = new int[HoldupItemCount];
        Incs = new int[HoldupItemCount];
        for (var i = 0; i < HoldupItemCount; i++)
        {
            ItemIds[i] = dispenser.holdupPackage[i].itemId;
            Counts[i] = dispenser.holdupPackage[i].count;
            Incs[i] = dispenser.holdupPackage[i].inc;
        }
    }

    public int PlanetId { get; }
    public int DispenserId { get; }
    public int HoldupItemCount { get; }
    public int[] ItemIds { get; }
    public int[] Counts { get; }
    public int[] Incs { get; }
}
