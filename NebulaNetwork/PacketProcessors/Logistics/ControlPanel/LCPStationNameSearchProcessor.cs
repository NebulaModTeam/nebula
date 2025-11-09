#region

using System.Collections.Generic;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPStationNameSearchProcessor : PacketProcessor<LCPStationNameSearchPacket>
{
    protected override void ProcessPacket(LCPStationNameSearchPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // Modify from UIStationRoutePanel.RefreshSearchEntries
            // For all ILS, find the station that custom name that match that search string
            var searchString = packet.SearchString;
            var isExact = packet.IsExact;
            var localPlanetId = packet.LocalPlanetId;
            var resultGids = new List<int>();
            var resultNames = new List<string>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var factories = GameMain.data.factories;
                var factoryCount = GameMain.data.factoryCount;
                for (var i = 0; i < factoryCount; i++)
                {
                    var planetFactory = factories[i];
                    var entityPool = planetFactory.entityPool;
                    var stationPool = planetFactory.transport.stationPool;
                    var buffer = planetFactory.digitalSystem.extraInfoes.buffer;
                    var cursor = planetFactory.digitalSystem.extraInfoes.cursor;
                    for (var j = 1; j < cursor; j++)
                    {
                        var extraInfoComponent = buffer[j];
                        if (extraInfoComponent != null && extraInfoComponent.objectType == EObjectType.None)
                        {
                            if (isExact && extraInfoComponent.info != searchString) continue;
                            if (!extraInfoComponent.info.Contains(searchString)) continue;
                            ref var ptr = ref entityPool[extraInfoComponent.objectId];
                            if (ptr.id == extraInfoComponent.objectId)
                            {
                                var stationComponent = stationPool[ptr.stationId];
                                if (stationComponent != null && stationComponent.id == ptr.stationId && stationComponent.isStellar && stationComponent.planetId != localPlanetId)
                                {
                                    resultGids.Add(stationComponent.gid);
                                    resultNames.Add(extraInfoComponent.info);
                                }
                            }
                        }
                    }
                }
            }
            packet.ResultGids = resultGids.ToArray();
            packet.ResultNames = resultNames.ToArray();
            conn.SendPacket(packet);
        }
        else // Client
        {
            UIStationRoutePanel panel = null;
            if (UIRoot.instance.uiGame.stationWindow.uiRoutePanel.active)
            {
                panel = UIRoot.instance.uiGame.stationWindow.uiRoutePanel;
            }
            if (UIRoot.instance.uiGame.controlPanelWindow.stationInspector.uiRoutePanel.active)
            {
                panel = UIRoot.instance.uiGame.controlPanelWindow.stationInspector.uiRoutePanel;
            }
            if (panel != null && panel.searchNameInputField.text == packet.SearchString)
            {
                if (packet.IsExact)
                {
                    // Continue from RefreshSearchEntries
                    panel.addGids.Clear();
                    panel.addGids.AddRange(packet.ResultGids);
                    panel.RefreshAddEntry();
                }
                else if (packet.ResultNames != null)
                {
                    // Modify from UIStationRoutePanel.OnSearchNameInputChanged
                    panel.searchDropDownRt.gameObject.SetActive(true);
                    panel.activeSearchEntryCount = 0;
                    var parent = panel.searchEntryPrefab.transform.parent;

                    for (var i = 0; i < packet.ResultNames.Length; i++)
                    {
                        if (panel.searchEntries.Count <= panel.activeSearchEntryCount)
                        {
                            panel.searchEntries.Add(null);
                        }
                        var entry = panel.searchEntries[panel.activeSearchEntryCount];
                        if (entry == null)
                        {
                            entry = Object.Instantiate(panel.searchEntryPrefab, parent);
                            entry._Create();
                            entry.rt.anchoredPosition = new Vector2(0f, (float)(-(float)panel.activeSearchEntryCount) * panel.searchEntryPrefab.rt.rect.height);
                            panel.searchEntries[panel.activeSearchEntryCount] = entry;
                        }
                        entry._Init(null);
                        entry._Open();
                        entry.realName = packet.ResultNames[i];
                        entry.SetName(packet.SearchString);
                        panel.activeSearchEntryCount++;
                    }

                    for (var i = panel.activeSearchEntryCount; i < panel.searchEntries.Count; i++)
                    {
                        panel.searchEntries[i]._Close();
                    }
                }
            }
        }
    }
}
