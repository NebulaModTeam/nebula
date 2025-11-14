#region

using System.Collections.Generic;
using System.Linq;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPStationRouteResultsProcessor : PacketProcessor<LCPStationRouteResultsPacket>
{
    protected override void ProcessPacket(LCPStationRouteResultsPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // Use the host's UI component for calculation and restore the data back afterward
            var panel = UIRoot.instance.uiGame.controlPanelWindow.stationInspector.uiRouteViewPanel;
            var oldIsRemoteRoute = panel.isRemoteRoute;
            var oldFactory = panel.inspectorFactory;
            var oldStation = panel.inspectorStation;
            var newFactory = GameMain.galaxy.PlanetById(packet.QueryPlanetId)?.factory;
            StationComponent newStation = null;
            if (packet.QueryStationGid < GameMain.data.galacticTransport.stationPool.Length)
            {
                newStation = GameMain.data.galacticTransport.stationPool[packet.QueryStationGid];
            }
            if (newFactory != null && newStation != null)
            {
                panel.isRemoteRoute = true;
                panel.SetData(newStation, newFactory);
                panel.DetermineRouteResult();
                ExportResults(panel, packet);

                panel.isRemoteRoute = oldIsRemoteRoute;
                if (oldFactory != null && oldStation != null)
                {
                    panel.SetData(oldStation, oldFactory);
                }
                else
                {
                    // SetData expect factory parameter is not null, so assign directly here
                    panel.inspectorFactory = null;
                    panel.inspectorStation = null;
                }
            }
            conn.SendPacket(packet);
        }
        else // Client
        {
            var panel = UIRoot.instance.uiGame.controlPanelWindow.stationInspector.uiRouteViewPanel;
            var factory = GameMain.galaxy.PlanetById(packet.QueryPlanetId)?.factory;
            StationComponent station = null;
            if (packet.QueryStationGid < GameMain.data.galacticTransport.stationPool.Length)
            {
                station = GameMain.data.galacticTransport.stationPool[packet.QueryStationGid];
            }
            if (panel.inspectorFactory != factory || panel.inspectorStation != station)
            {
                return;
            }
            panel.ResetEntryPool();
            panel.ClearResult();
            ImportResults(panel, packet);
            for (var i = 0; i < panel.resultPositions.Count; i++)
            {
                // resultEntries should have same length as resultPositions
                panel.resultEntries.Add(null);
            }
            panel.needDetermineEntryVisible = true;
        }
    }

    private static void ExportResults(UIControlPanelStationRouteViewPanel panel, LCPStationRouteResultsPacket packet)
    {
        packet.RouteResultCounts = panel.routeResultCounts.ToArray();
        packet.ResultPositions = panel.resultPositions.ToArray();

        var length = panel.routeResults.Count;
        packet.TargetIds = new int[length];
        packet.PairStorageIndexMasks = new int[length];
        packet.PlanetIds = new int[length];
        packet.RouteTypes = new short[length];
        packet.StationIds = new int[length];
        packet.DistanceToInspectorStationSqrs = new double[length];
        for (var i = 0; i < length; i++)
        {
            var result = panel.routeResults[i];
            packet.TargetIds[i] = result.targetId;
            packet.PairStorageIndexMasks[i] = result.pairStorageIndexMask;
            packet.PlanetIds[i] = result.planetId;
            packet.RouteTypes[i] = (short)result.routeType;
            packet.StationIds[i] = result.stationId;
            packet.DistanceToInspectorStationSqrs[i] = result.distanceToInspectorStationSqr;
        }
    }

    private static void ImportResults(UIControlPanelStationRouteViewPanel panel, LCPStationRouteResultsPacket packet)
    {
        if (packet.RouteResultCounts == null) return;

        panel.routeResultCounts = packet.RouteResultCounts;
        panel.resultPositions = new List<int>(packet.ResultPositions);

        var length = packet.TargetIds.Length;
        panel.routeResults = new List<StationRouteResult>(length);
        for (var i = 0; i < length; i++)
        {
            var result = new StationRouteResult
            {
                targetId = packet.TargetIds[i],
                pairStorageIndexMask = packet.PairStorageIndexMasks[i],
                planetId = packet.PlanetIds[i],
                routeType = (EUIControlPanelRouteType)packet.RouteTypes[i],
                stationId = packet.StationIds[i],
                distanceToInspectorStationSqr = packet.DistanceToInspectorStationSqrs[i]
            };
            panel.routeResults.Add(result);
        }
    }
}
