#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPStationEntryUpdateProcessor : PacketProcessor<LCPStationEntryUpdate>
{
    static readonly StationStore[] stationStores = new StationStore[5];

    protected override void ProcessPacket(LCPStationEntryUpdate packet, NebulaConnection conn)
    {
        if (IsHost) return;

        var objectEntryPool = UIRoot.instance.uiGame.controlPanelWindow.objectEntryPool;
        for (var i = 0; i < objectEntryPool.Count; i++)
        {
            if (objectEntryPool[i] != null && objectEntryPool[i].index == packet.Index)
            {
                var entry = objectEntryPool[i] as UIControlPanelStationEntry;
                UpdateEntry(entry, packet);
                return;
            }
        }
    }

    private static void UpdateEntry(UIControlPanelStationEntry entry, LCPStationEntryUpdate packet)
    {
        if (entry == null) return;

        // Modify from UIControlPanelStationEntry._OnUpdate
        // Update item UI
        for (var i = 0; i < 5; i++)
        {
            ref var store = ref stationStores[i];
            store.itemId = packet.ItemId[i];
            store.count = packet.ItemCount[i];
            store.localOrder = packet.LocalOrder[i];
            store.remoteOrder = packet.RemoteOrder[i];
            store.max = packet.StoreMax[i];
            store.localLogic = (ELogisticStorage)packet.LocalLogic[i];
            store.remoteLogic = (ELogisticStorage)packet.RemoteLogic[i];
        }
        var tmp = entry.station.storage;
        entry.station.storage = stationStores;
        // expand entry.UpdateItems()
        entry.storageItem0.SetVisible(stationStores[0].itemId > 0);
        entry.storageItem0._Update();
        entry.storageItem1.SetVisible(stationStores[1].itemId > 0);
        entry.storageItem1._Update();
        entry.storageItem2.SetVisible(stationStores[2].itemId > 0);
        entry.storageItem2._Update();
        entry.storageItem3.SetVisible(stationStores[3].itemId > 0);
        entry.storageItem3._Update();
        entry.storageItem4.SetVisible(stationStores[4].itemId > 0);
        entry.storageItem4._Update();
        entry.station.storage = tmp;

        // Update drone and ship count UI
        switch (entry.target.entryType)
        {
            case EControlPanelEntryType.InterstellarStation:
                entry.SetDeliveryVisible(true);
                entry.droneCountSB.Append(packet.IdleDroneCount);
                entry.droneCountSB.Append('/');
                entry.droneCountSB.Append(packet.IdleDroneCount + packet.WorkDroneCount);
                entry.droneCountText.text = entry.droneCountSB.ToString();
                entry.droneCountSB.Clear();
                entry.shipCountSB.Append(packet.IdleShipCount);
                entry.shipCountSB.Append('/');
                entry.shipCountSB.Append(packet.IdleShipCount + packet.WorkShipCount);
                entry.shipCountText.text = entry.shipCountSB.ToString();
                entry.shipCountSB.Clear();
                entry.warperCountText.text = packet.WarperCount.ToString();
                entry.fillNecessaryButton.button.interactable = true;
                entry.fillNecessaryImage.raycastTarget = true;
                break;

            case EControlPanelEntryType.OrbitCollector:
                entry.SetDeliveryVisible(false);
                entry.droneCountText.text = "";
                entry.shipCountText.text = "";
                entry.warperCountText.text = "";
                entry.fillNecessaryButton.button.interactable = false;
                entry.fillNecessaryImage.raycastTarget = false;
                break;

            case EControlPanelEntryType.VeinCollector:
                entry.SetDeliveryVisible(false);
                entry.droneIconImage.raycastTarget = true;
                entry.droneIconImage.color = entry.masterWindow.deliveryIconColor;
                entry.droneCountText.color = entry.masterWindow.deliveryTextColor;
                entry.droneCountSB.Append(packet.IdleDroneCount);
                entry.droneCountSB.Append('/');
                entry.droneCountSB.Append(packet.IdleDroneCount + packet.WorkDroneCount);
                entry.droneCountText.text = entry.droneCountSB.ToString();
                entry.droneCountSB.Clear();
                entry.shipCountText.text = "";
                entry.warperCountText.text = "";
                entry.fillNecessaryButton.button.interactable = true;
                entry.fillNecessaryImage.raycastTarget = true;
                break;

            case EControlPanelEntryType.LocalStation:
                entry.SetDeliveryVisible(false);
                entry.droneCountText.text = "";
                entry.shipCountText.text = "";
                entry.warperCountText.text = "";
                entry.fillNecessaryButton.button.interactable = false;
                entry.fillNecessaryImage.raycastTarget = false;
                break;
        }

        // Update power UI
        entry.powerGroupGo.SetActive(entry.target.entryType != EControlPanelEntryType.OrbitCollector);
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
