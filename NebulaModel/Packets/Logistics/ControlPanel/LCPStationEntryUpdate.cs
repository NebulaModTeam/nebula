using UnityEngine;

namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPStationEntryUpdate
{
    public LCPStationEntryUpdate() { }

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
        else
        {
            ConsumerRatio = 1.0f;
            RequirePower = 0;
            WorkEnergyPerTick = 0;
            PowerRound = 0;
        }

        IdleDroneCount = (short)station.idleDroneCount;
        WorkDroneCount = (short)station.workDroneCount;
        IdleShipCount = (short)station.idleShipCount;
        WorkShipCount = (short)station.workShipCount;
        WarperCount = (short)station.warperCount;

        if (ItemId == null)
        {
            ItemId = new int[5];
            ItemCount = new int[5];
            LocalOrder = new short[5];
            RemoteOrder = new short[5];
            StoreMax = new int[5];
            LocalLogic = new byte[5];
            RemoteLogic = new byte[5];
        }
        var i = 0;
        if (station.storage != null)
        {
            var storeLength = station.storage.Length;
            if (storeLength > 5) storeLength = 5;
            for (; i < storeLength; i++)
            {
                ref var store = ref station.storage[i];
                ItemId[i] = store.itemId;
                ItemCount[i] = store.count;
                LocalOrder[i] = (short)Mathf.Clamp(store.localOrder, -20000, 20000);
                RemoteOrder[i] = (short)Mathf.Clamp(store.remoteOrder, -20000, 20000);
                StoreMax[i] = store.max;
                LocalLogic[i] = (byte)store.localLogic;
                RemoteLogic[i] = (byte)store.remoteLogic;
            }
        }
        for (; i < 5; i++)
        {
            ItemId[i] = 0;
            ItemCount[i] = 0;
        }
    }

    public static readonly LCPStationEntryUpdate Instance = new();

    public int Index { get; set; }
    public float ConsumerRatio { get; set; }
    public long RequirePower { get; set; }
    public long WorkEnergyPerTick { get; set; }
    public float PowerRound { get; set; }

    public short IdleDroneCount { get; set; }
    public short WorkDroneCount { get; set; }
    public short IdleShipCount { get; set; }
    public short WorkShipCount { get; set; }
    public short WarperCount { get; set; }

    public int[] ItemId { get; set; }
    public int[] ItemCount { get; set; }
    public short[] LocalOrder { get; set; }
    public short[] RemoteOrder { get; set; }
    public int[] StoreMax { get; set; }
    public byte[] LocalLogic { get; set; }
    public byte[] RemoteLogic { get; set; }
}
