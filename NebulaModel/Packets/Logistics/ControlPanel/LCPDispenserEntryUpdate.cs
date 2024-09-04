namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPDispenserEntryUpdate
{
    public LCPDispenserEntryUpdate() { }

    public void Set(int index, DispenserComponent dispenser, PlanetFactory factory)
    {
        Index = index;

        var powerSystem = factory.powerSystem;
        var powerConsumerComponent = powerSystem.consumerPool[dispenser.pcId];
        var networkId = powerConsumerComponent.networkId;
        var powerNetwork = powerSystem.netPool[networkId];
        ConsumerRatio = (float)powerNetwork.consumerRatio;
        RequirePower = (long)((powerConsumerComponent.requiredEnergy * 60L) * ConsumerRatio);
        WorkEnergyPerTick = powerSystem.consumerPool[dispenser.pcId].workEnergyPerTick;
        PowerRound = dispenser.energy / (float)dispenser.energyMax;

        Filter = dispenser.filter;
        PlayerMode = (short)dispenser.playerMode;
        StorageMode = (short)dispenser.storageMode;
        IdleCourierCount = dispenser.idleCourierCount;
        WorkCourierCount = dispenser.workCourierCount;

        var status = 0;
        if (dispenser.playerMode == EPlayerDeliveryMode.None && dispenser.storageMode == EStorageDeliveryMode.None)
        {
            status = 1;
        }
        else if (dispenser.filter == 0)
        {
            status = 2;
        }
        else if (dispenser.idleCourierCount + dispenser.workCourierCount == 0)
        {
            status = 3;
        }
        else if (dispenser.holdupItemCount > 0)
        {
            status = 5;
        }
        else if (ConsumerRatio < 0.0001f && dispenser.energy < 100000L)
        {
            status = 4;
        }
        WarningFlag = status > 0;

        var count = 0;
        var inc = 0;
        var loop = 0;
        if (dispenser.storage != null && dispenser.filter > 0)
        {
            var storageComponent = dispenser.storage;
            do
            {
                count += storageComponent.GetItemCount(dispenser.filter, out var num);
                inc += num;
                storageComponent = storageComponent.previousStorage;
                if (loop++ > 20) break;
            }
            while (storageComponent != null);
        }
        ItemCount = count;
    }

    public static readonly LCPDispenserEntryUpdate Instance = new();

    public int Index { get; set; }
    public float ConsumerRatio { get; set; }
    public long RequirePower { get; set; }
    public long WorkEnergyPerTick { get; set; }
    public float PowerRound { get; set; }

    public int Filter { get; set; }
    public short PlayerMode { get; set; }
    public short StorageMode { get; set; }
    public int IdleCourierCount { get; set; }
    public int WorkCourierCount { get; set; }
    public bool WarningFlag { get; set; }
    public int ItemCount { get; set; }
}
