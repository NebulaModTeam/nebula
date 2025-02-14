#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UITools;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPObjectEntryEntityInfoProcessor : PacketProcessor<LCPObjectEntryEntityInfo>
{
    protected override void ProcessPacket(LCPObjectEntryEntityInfo packet, NebulaConnection conn)
    {
        if (IsHost) return;

        var objectEntryPool = UIRoot.instance.uiGame.controlPanelWindow.objectEntryPool;
        for (var i = 0; i < objectEntryPool.Count; i++)
        {
            if (objectEntryPool[i] != null && objectEntryPool[i].index == packet.Index)
            {
                UpdateEntry(objectEntryPool[i], packet);
                return;
            }
        }
    }

    private static void UpdateEntry(UIControlPanelObjectEntry objectEntry, LCPObjectEntryEntityInfo packet)
    {
        if (objectEntry == null) return;

        if (objectEntry is UIControlPanelStationEntry)
        {
            var entry = objectEntry as UIControlPanelStationEntry;

            entry.storageItem0.station = entry.station;
            entry.storageItem1.station = entry.station;
            entry.storageItem2.station = entry.station;
            entry.storageItem3.station = entry.station;
            entry.storageItem4.station = entry.station;
            var itemProto = LDB.items.Select(packet.ProtoId);
            entry.stationIcon.sprite = itemProto?.iconSprite;
            entry.stationIdText.text = "#" + packet.Id;
            entry.stationIconButton.tips.itemId = packet.ProtoId;
            entry.stationIconButton.tips.itemInc = 0;
            entry.stationIconButton.tips.itemCount = 0;
            entry.stationIconButton.tips.type = UIButton.ItemTipType.Other;

            entry.id = packet.Id;
            var text = "无名称编号".Translate();
            if (packet.Name == "")
            {
                text = string.Format(text, entry.id.ToString());
                entry.stationNameText.color = entry.masterWindow.unnamedColor;
            }
            else
            {
                text = packet.Name;
                entry.stationNameText.color = entry.masterWindow.renamedColor;
            }
            Utils.UITextTruncateShow(entry.stationNameText, ref text, entry.stationNameTextWidthLimit);
        }
        else if (objectEntry is UIControlPanelAdvancedMinerEntry)
        {
            var entry = objectEntry as UIControlPanelAdvancedMinerEntry;

            entry.storageItem.station = entry.station;
            var itemProto = LDB.items.Select(packet.ProtoId);
            entry.stationIcon.sprite = (itemProto?.iconSprite);
            entry.stationIdText.text = "#" + packet.Id;
            entry.stationIconButton.tips.itemId = packet.ProtoId;
            entry.stationIconButton.tips.itemInc = 0;
            entry.stationIconButton.tips.itemCount = 0;
            entry.stationIconButton.tips.type = UIButton.ItemTipType.Other;

            entry.id = packet.Id;
            var text = "无名称编号".Translate();
            if (packet.Name == "")
            {
                text = string.Format(text, entry.id.ToString());
                entry.stationNameText.color = entry.masterWindow.unnamedColor;
            }
            else
            {
                text = packet.Name;
                entry.stationNameText.color = entry.masterWindow.renamedColor;
            }
            Utils.UITextTruncateShow(entry.stationNameText, ref text, entry.stationNameTextWidthLimit);
        }
        else if (objectEntry is UIControlPanelDispenserEntry)
        {
            var entry = objectEntry as UIControlPanelDispenserEntry;

            entry.id = packet.Id;
            var itemProto = LDB.items.Select(packet.ProtoId);
            entry.dispenserIcon.sprite = itemProto?.iconSprite;
            entry.dispenserIdText.text = "#" + entry.id.ToString();
            entry.dispenserIconButton.tips.itemId = packet.ProtoId;
            entry.dispenserIconButton.tips.itemInc = 0;
            entry.dispenserIconButton.tips.itemCount = 0;
            entry.dispenserIconButton.tips.type = UIButton.ItemTipType.Other;

            var text = "无名称编号".Translate();
            entry.dispenserNameText.text = string.Format(text, entry.id.ToString());
        }
    }
}
