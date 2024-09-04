#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPDispenserEntryUpdateProcessor : PacketProcessor<LCPDispenserEntryUpdate>
{
    protected override void ProcessPacket(LCPDispenserEntryUpdate packet, NebulaConnection conn)
    {
        if (IsHost) return;

        var objectEntryPool = UIRoot.instance.uiGame.controlPanelWindow.objectEntryPool;
        for (var i = 0; i < objectEntryPool.Count; i++)
        {
            if (objectEntryPool[i] != null && objectEntryPool[i].index == packet.Index)
            {
                var entry = objectEntryPool[i] as UIControlPanelDispenserEntry;
                UpdateEntry(entry, packet);
                return;
            }
        }
    }

    private static void UpdateEntry(UIControlPanelDispenserEntry entry, LCPDispenserEntryUpdate packet)
    {
        if (entry == null) return;

        // Modify from UIControlPanelDispenserEntry._OnUpdate
        var filter = packet.Filter;
        var playerMode = (EPlayerDeliveryMode)packet.PlayerMode;
        var storageMode = (EStorageDeliveryMode)packet.StorageMode;

        if (playerMode == EPlayerDeliveryMode.Recycle && filter < 0)
        {
            entry.transitItemGroup.alpha = 0f;
            entry.transitItemGroup.blocksRaycasts = false;
            entry.recycleAllText.color = entry.masterWindow.recycleAllColor;
        }
        else
        {
            entry.transitItemGroup.alpha = 1f;
            entry.transitItemGroup.blocksRaycasts = true;
            entry.transitItemText.color = entry.masterWindow.transitItemTextColor;
            entry.recycleAllText.color = Color.clear;
            var itemProto = LDB.items.Select(filter);
            if (itemProto != null)
            {
                entry.transitItemImage.sprite = itemProto.iconSprite;
                entry.transitItemButton.tips.itemId = filter;
                entry.transitItemButton.tips.itemInc = 0;
                entry.transitItemButton.tips.itemCount = 0;
                entry.transitItemButton.tips.type = UIButton.ItemTipType.Other;
                entry.transitItemText.text = packet.ItemCount.ToString();
            }
            else
            {
                entry.transitItemGroup.alpha = 0f;
                entry.transitItemGroup.blocksRaycasts = false;
                entry.recycleAllText.color = Color.clear;
            }
        }
        entry.SetPlayerDeliveryActiveModes(playerMode, entry.mechaDeliverySupply, entry.mechaDeliveryDemand, entry.mechaDeliverySupplyDemand);
        entry.SetStorageDeliveryActiveModes(storageMode, entry.storageDeliverySupply, entry.storageDeliveryDemand);
        entry.courierCountText.text = packet.IdleCourierCount.ToString() + "/" + (packet.IdleCourierCount + packet.WorkCourierCount).ToString();

        // Update power UI
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
        entry.warningItemCanvasGroup.alpha = packet.WarningFlag ? 1 : 0;
    }
}
