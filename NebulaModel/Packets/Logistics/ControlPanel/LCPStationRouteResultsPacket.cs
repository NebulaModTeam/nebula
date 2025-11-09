namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPStationRouteResultsPacket
{
    public LCPStationRouteResultsPacket() { }

    public int QueryPlanetId { get; set; }
    public int QueryStationGid { get; set; }

    // Return routeResultCounts, resultPositions, routeResults
    public int[] RouteResultCounts { get; set; }
    public int[] ResultPositions { get; set; }

    // Members of StationRouteResult
    public int[] TargetIds { get; set; }
    public int[] PairStorageIndexMasks { get; set; }
    public int[] PlanetIds { get; set; }
    public short[] RouteTypes { get; set; }
    public int[] StationIds { get; set; }
    public double[] DistanceToInspectorStationSqrs { get; set; }
}
