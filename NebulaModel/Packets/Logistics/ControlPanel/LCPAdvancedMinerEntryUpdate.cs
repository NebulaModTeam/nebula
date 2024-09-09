using UnityEngine;

namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPAdvancedMinerEntryUpdate
{
    public LCPAdvancedMinerEntryUpdate() { }

    public void Set(int index, StationComponent station, PlanetFactory factory)
    {
        Index = index;

        if (!station.isCollector)
        {
            var powerSystem = factory.powerSystem;
            var powerConsumerComponent = powerSystem.consumerPool[station.pcId];
            var networkId = powerConsumerComponent.networkId;
            var powerNetwork = powerSystem.netPool[networkId];
            ConsumerRatio = (float)powerNetwork.consumerRatio;
            RequirePower = (long)((powerConsumerComponent.requiredEnergy * 60L) * ConsumerRatio);
            WorkEnergyPerTick = powerSystem.consumerPool[station.pcId].workEnergyPerTick;
            PowerRound = station.energy / (float)station.energyMax;
        }

        var minerComponent = factory.factorySystem.minerPool[station.minerId];
        if (minerComponent.id == station.minerId)
        {
            var veinPool = factory.veinPool;
            var lowVeinCount = 0;
            var emptyVeinCount = 0;
            if (minerComponent.veinCount > 0)
            {
                for (var i = 0; i < minerComponent.veinCount; i++)
                {
                    var id = minerComponent.veins[i];
                    if (id > 0 && veinPool[id].id == id)
                    {
                        var amount = veinPool[id].amount;
                        if (amount < 1000)
                        {
                            if (amount > 0)
                            {
                                lowVeinCount++;
                            }
                            else
                            {
                                emptyVeinCount++;
                            }
                        }
                    }
                }
            }
            VeinCount = (short)minerComponent.veinCount;
            LowVeinCount = (short)lowVeinCount;
            EmptyVeinCount = (short)emptyVeinCount;

            var veinId = ((minerComponent.veinCount == 0) ? 0 : minerComponent.veins[minerComponent.currentVeinIndex]);
            VeinProtoId = (int)veinPool[veinId].type;
            var veinProto = LDB.veins.Select((int)veinPool[veinId].type);
            if (veinProto != null)
            {
                minerComponent.GetTotalVeinAmount(veinPool);
                TotalVeinAmount = minerComponent.totalVeinAmount;
            }
        }

        ItemId = 0;
        if (station.storage == null || station.storage.Length == 0) return;
        ref var store = ref station.storage[0];
        ItemId = store.itemId;
        ItemCount = store.count;
        LocalOrder = (short)Mathf.Clamp(store.localOrder, -20000, 20000);
        RemoteOrder = (short)Mathf.Clamp(store.remoteOrder, -20000, 20000);
        StoreMax = store.max;
        LocalLogic = (byte)store.localLogic;
        RemoteLogic = (byte)store.remoteLogic;
    }

    public static readonly LCPAdvancedMinerEntryUpdate Instance = new();

    public int Index { get; set; }
    public float ConsumerRatio { get; set; }
    public long RequirePower { get; set; }
    public long WorkEnergyPerTick { get; set; }
    public float PowerRound { get; set; }

    public int VeinProtoId { get; set; }
    public long TotalVeinAmount { get; set; }
    public short VeinCount { get; set; }
    public short LowVeinCount { get; set; }
    public short EmptyVeinCount { get; set; }

    public int ItemId { get; set; }
    public int ItemCount { get; set; }
    public short LocalOrder { get; set; }
    public short RemoteOrder { get; set; }
    public int StoreMax { get; set; }
    public byte LocalLogic { get; set; }
    public byte RemoteLogic { get; set; }
}
