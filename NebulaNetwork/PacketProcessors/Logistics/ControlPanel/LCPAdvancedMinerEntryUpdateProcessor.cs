#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPAdvancedMinerEntryUpdateProcessor : PacketProcessor<LCPAdvancedMinerEntryUpdate>
{
    static readonly StationStore[] stationStores = new StationStore[1];

    protected override void ProcessPacket(LCPAdvancedMinerEntryUpdate packet, NebulaConnection conn)
    {
        if (IsHost) return;

        var objectEntryPool = UIRoot.instance.uiGame.controlPanelWindow.objectEntryPool;
        for (var i = 0; i < objectEntryPool.Count; i++)
        {
            if (objectEntryPool[i] != null && objectEntryPool[i].index == packet.Index)
            {
                var entry = objectEntryPool[i] as UIControlPanelAdvancedMinerEntry;
                UpdateEntry(entry, packet);
                return;
            }
        }
    }

    private static void UpdateEntry(UIControlPanelAdvancedMinerEntry entry, LCPAdvancedMinerEntryUpdate packet)
    {
        if (entry == null) return;

        // Modify from UIControlPanelAdvancedMinerEntry._OnUpdate
        var isInfiniteResource = GameMain.data.gameDesc.isInfiniteResource;
        var isAllEmpty = packet.VeinCount == packet.EmptyVeinCount;
        entry.warningItemCanvasGroup.alpha = ((packet.LowVeinCount > 0 && !isInfiniteResource) || isAllEmpty) ? 1 : 0;
        entry.warningText.text = (isAllEmpty ? "矿脉全部耗尽警告".Translate() : string.Format("矿脉耗尽警告".Translate(), packet.LowVeinCount));

        var veinProto = LDB.veins.Select(packet.VeinProtoId);
        if (!isAllEmpty && veinProto != null)
        {
            entry.minerItemImage.sprite = veinProto.iconSprite;
            entry.minerItemImage.color = Color.white;
            StringBuilderUtility.WriteKMG(entry.veinAmountSB, 8, packet.TotalVeinAmount, false);
            entry.veinAmountText.text = (isInfiniteResource ? ("(" + packet.VeinCount + ")") : string.Concat(new object[]
            {
                    entry.veinAmountSB.ToString(),
                    "(",
                    packet.VeinCount,
                    ")"
            }));
        }
        else
        {
            entry.minerItemImage.sprite = null;
            entry.minerItemImage.color = Color.clear;
            entry.veinAmountText.text = "";
        }

        // Update item UI
        ref var store = ref stationStores[0];
        store.itemId = packet.ItemId;
        store.count = packet.ItemCount;
        store.localOrder = packet.LocalOrder;
        store.remoteOrder = packet.RemoteOrder;
        store.max = packet.StoreMax;
        store.localLogic = (ELogisticStorage)packet.LocalLogic;
        store.remoteLogic = (ELogisticStorage)packet.RemoteLogic;
        var tmp = entry.station.storage;
        entry.station.storage = stationStores;
        // expand UpdateItems()
        entry.storageItem.SetVisible(stationStores[0].itemId > 0);
        entry.storageItem._Update();
        entry.station.storage = tmp;

        // Update power UI
        entry.powerGroupGo.SetActive(true);
        var consumerRatio = (float)packet.ConsumerRatio;
        int powerStatusCode;
        if (consumerRatio >= 1f)
        {
            entry.powerCircleFg.fillAmount = 1f;
            powerStatusCode = 1;
        }
        else
        {
            entry.powerCircleFg.fillAmount = consumerRatio;
            powerStatusCode = ((consumerRatio >= 0.1f) ? 2 : 3);
        }
        StringBuilderUtility.WriteKMG1000(entry.sbw, 8, packet.RequirePower, true);
        StringBuilderUtility.WriteKMG1000(entry.sbw2, 8, packet.WorkEnergyPerTick * 60L, true);
        entry.powerText.text = entry.sbw.ToString();
        entry.maxChargePowerValue.text = entry.sbw2.ToString();
        entry.powerRoundFg.fillAmount = packet.PowerRound;
        switch (powerStatusCode)
        {

            case 1:
                entry.powerSignImage.color = entry.masterWindow.powerSignColor1;
                entry.powerCircleBg.color = entry.masterWindow.powerCircleBgColor1;
                entry.powerCircleFg.color = entry.masterWindow.powerCircleFgColor1;
                entry.powerRoundFg.color = entry.masterWindow.powerRoundFgColor1;
                entry.powerText.color = entry.masterWindow.powerTextColor1;
                entry.maxChargePowerValue.color = entry.masterWindow.powerTextColor1;
                break;
            case 2:
                entry.powerSignImage.color = entry.masterWindow.powerSignColor2;
                entry.powerCircleBg.color = entry.masterWindow.powerCircleBgColor2;
                entry.powerCircleFg.color = entry.masterWindow.powerCircleFgColor2;
                entry.powerRoundFg.color = entry.masterWindow.powerRoundFgColor2;
                entry.powerText.color = entry.masterWindow.powerTextColor2;
                entry.maxChargePowerValue.color = entry.masterWindow.powerTextColor2;
                break;
            case 3:
                entry.powerSignImage.color = entry.masterWindow.powerSignColor3;
                entry.powerCircleBg.color = entry.masterWindow.powerCircleBgColor3;
                entry.powerCircleFg.color = entry.masterWindow.powerCircleFgColor3;
                entry.powerRoundFg.color = entry.masterWindow.powerRoundFgColor3;
                entry.powerText.color = entry.masterWindow.powerTextColor3;
                entry.maxChargePowerValue.color = entry.masterWindow.powerTextColor3;
                break;
            default:
                entry.powerSignImage.color = entry.masterWindow.powerSignColor0;
                entry.powerCircleBg.color = entry.masterWindow.powerCircleBgColor0;
                entry.powerCircleFg.color = entry.masterWindow.powerCircleFgColor0;
                entry.powerRoundFg.color = entry.masterWindow.powerRoundFgColor0;
                entry.powerText.color = entry.masterWindow.powerTextColor0;
                entry.maxChargePowerValue.color = entry.masterWindow.powerTextColor0;
                break;
        }
    }
}
