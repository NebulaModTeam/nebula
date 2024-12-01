#region

using System;
using System.Collections.Generic;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPFilterResultsRequestProcessor : PacketProcessor<LCPFilterResultsRequest>
{
    protected override void ProcessPacket(LCPFilterResultsRequest packet, NebulaConnection conn)
    {
        if (IsClient) return;

        // Modify from UIControlPanelWindow.DetermineFilterResults
        // Send back the filter result from client's request

        var filter = new ControlPanelFilter
        {
            typeFilter = (ControlPanelFilter.EEntryFilter)packet.TypeFilter,
            astroFilter = packet.AstorFilter,
            itemsFilter = packet.ItemsFilter,
            stateFilter = packet.StateFilter,
            searchFilter = packet.SearchFilter,
            sortMethod = (ControlPanelFilter.ESortMethod)packet.SortMethod
        };

        List<ControlPanelAstroData> sortedAstros = [];
        List<ControlPanelTarget> targets = [];


        var factoryCount = GameMain.data.factoryCount;
        var localPlanetAstroId = packet.LocalPlanetAstroId;
        var localStarAstroId = packet.LocalStarAstroId;
        var playerUPosition = new VectorLF3(packet.PlayerUposition.x, packet.PlayerUposition.y, packet.PlayerUposition.z);

        var factories = GameMain.data.factories;
        var galaxyAstros = GameMain.data.spaceSector.galaxyAstros;
        for (var i = 0; i < factoryCount; i++)
        {
            var astroId = factories[i].planet.astroId;
            if (filter.sortMethod == ControlPanelFilter.ESortMethod.AstroDistance)
            {
                var sqrMagnitude = (galaxyAstros[astroId].uPos - playerUPosition).sqrMagnitude;
                var index = sortedAstros.Count - 1;
                while (index >= 0 && sqrMagnitude < sortedAstros[index].sqrDistToPlayer)
                {
                    index--;
                }
                sortedAstros.Insert(index + 1, new ControlPanelAstroData(astroId, sqrMagnitude));
            }
            else
            {
                sortedAstros.Add(new ControlPanelAstroData(astroId));
            }
        }

        var hasStationType = filter.hasStationType;
        var flag1 = (filter.typeFilter & ControlPanelFilter.EEntryFilter.InterstellarStation) > ControlPanelFilter.EEntryFilter.None;
        var flag2 = (filter.typeFilter & ControlPanelFilter.EEntryFilter.OrbitCollector) > ControlPanelFilter.EEntryFilter.None;
        var flag3 = (filter.typeFilter & ControlPanelFilter.EEntryFilter.LocalStation) > ControlPanelFilter.EEntryFilter.None;
        var flag4 = (filter.typeFilter & ControlPanelFilter.EEntryFilter.VeinCollector) > ControlPanelFilter.EEntryFilter.None;
        var flag5 = (filter.typeFilter & (ControlPanelFilter.EEntryFilter.LocalStation | ControlPanelFilter.EEntryFilter.VeinCollector)) > ControlPanelFilter.EEntryFilter.None;
        var flag6 = !flag5;
        var flag7 = (filter.typeFilter & ControlPanelFilter.EEntryFilter.Dispenser) > ControlPanelFilter.EEntryFilter.None;
        var flag8 = !string.IsNullOrWhiteSpace(filter.searchFilter);
        var num3 = 0;
        var flag9 = flag8 && int.TryParse(filter.searchFilter, out num3);
        var flag10 = filter.stateFilter != -1;
        var hasItemFilter = filter.hasItemFilter;

        var astrosFactory = GameMain.data.galaxy.astrosFactory;
        for (var j = 0; j < sortedAstros.Count; j++)
        {
            var planetFactory = astrosFactory[sortedAstros[j].astroId];
            if (planetFactory != null)
            {
                var planet = planetFactory.planet;
                if (planet != null && (filter.astroFilter == -1 || (filter.astroFilter == 0 && (planet.astroId == localPlanetAstroId || (localPlanetAstroId == 0 && planet.star.astroId == localStarAstroId))) || filter.astroFilter == planet.astroId || filter.astroFilter == planet.star.astroId))
                {
                    var controlPanelTarget = new ControlPanelTarget(EObjectType.None, 0, planet.astroId, EControlPanelEntryType.None);
                    targets.Add(controlPanelTarget);
                    var count = targets.Count;
                    if (hasStationType)
                    {
                        var stationPool = planetFactory.transport.stationPool;
                        var stationCursor = planetFactory.transport.stationCursor;
                        var entityPool = planetFactory.entityPool;
                        var buffer = planetFactory.digitalSystem.extraInfoes.buffer;
                        for (var k = 1; k < stationCursor; k++)
                        {
                            var stationComponent = stationPool[k];
                            if (stationComponent != null && stationComponent.id == k)
                            {
                                var econtrolPanelEntryType = (stationComponent.isStellar ? (stationComponent.isCollector ? EControlPanelEntryType.OrbitCollector : EControlPanelEntryType.InterstellarStation) : (stationComponent.isVeinCollector ? EControlPanelEntryType.VeinCollector : EControlPanelEntryType.LocalStation));
                                if ((flag1 && econtrolPanelEntryType == EControlPanelEntryType.InterstellarStation) || (flag2 && econtrolPanelEntryType == EControlPanelEntryType.OrbitCollector) || (flag3 && econtrolPanelEntryType == EControlPanelEntryType.LocalStation) || (flag4 && econtrolPanelEntryType == EControlPanelEntryType.VeinCollector))
                                {
                                    var flag12 = true;
                                    if (flag8)
                                    {
                                        flag12 = flag9 && stationComponent.id == num3;
                                        if (!flag12)
                                        {
                                            var extraInfoId = entityPool[stationComponent.entityId].extraInfoId;
                                            if (extraInfoId > 0)
                                            {
                                                var info = buffer[extraInfoId].info;
                                                if (!string.IsNullOrEmpty(info) && info.IndexOf(filter.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                                                {
                                                    flag12 = true;
                                                }
                                            }
                                        }
                                    }
                                    if (flag12)
                                    {
                                        var flag13 = true;
                                        if (hasItemFilter || flag10)
                                        {
                                            var storage = stationComponent.storage;
                                            flag13 = false;
                                            for (var l = 0; l < storage.Length; l++)
                                            {
                                                if ((!hasItemFilter || filter.ItemFilterPass(storage[l].itemId)) && (!flag10 || (storage[l].itemId > 0 && ((flag6 && storage[l].remoteLogic == (ELogisticStorage)filter.stateFilter) || (flag5 && storage[l].localLogic == (ELogisticStorage)filter.stateFilter)))))
                                                {
                                                    flag13 = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (flag13)
                                        {
                                            var controlPanelTarget2 = new ControlPanelTarget(EObjectType.None, stationComponent.entityId, planet.astroId, econtrolPanelEntryType);
                                            targets.Add(controlPanelTarget2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag7)
                    {
                        var dispenserPool = planetFactory.transport.dispenserPool;
                        var dispenserCursor = planetFactory.transport.dispenserCursor;
                        for (var m = 1; m < dispenserCursor; m++)
                        {
                            var dispenserComponent = dispenserPool[m];
                            if (dispenserComponent != null && dispenserComponent.id == m)
                            {
                                var flag14 = true;
                                if (flag8 && flag9)
                                {
                                    flag14 = dispenserComponent.id == num3;
                                }
                                if (flag14)
                                {
                                    var flag15 = true;
                                    if (hasItemFilter || flag10)
                                    {
                                        flag15 = false;
                                        if ((!hasItemFilter || filter.ItemFilterPass(dispenserComponent.filter) || (dispenserComponent.filter < 0 && dispenserComponent.playerMode == EPlayerDeliveryMode.Recycle)) && (!flag10 || (dispenserComponent.storageMode > EStorageDeliveryMode.None && (filter.stateFilter == 0 || filter.stateFilter == (int)dispenserComponent.storageMode)) || (dispenserComponent.playerMode > EPlayerDeliveryMode.None && filter.stateFilter - (int)dispenserComponent.playerMode - 9 <= 2)))
                                        {
                                            flag15 = true;
                                        }
                                    }
                                    if (flag15)
                                    {
                                        var controlPanelTarget3 = new ControlPanelTarget(EObjectType.None, dispenserComponent.entityId, planet.astroId, EControlPanelEntryType.Dispenser);
                                        targets.Add(controlPanelTarget3);
                                    }
                                }
                            }
                        }
                    }
                    if (targets.Count == count)
                    {
                        targets.RemoveAt(targets.Count - 1);
                    }
                }
            }
        }

        conn.SendPacket(new LCPFilterResultsResponse(sortedAstros, targets));
    }
}
