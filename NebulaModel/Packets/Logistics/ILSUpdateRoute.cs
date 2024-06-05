namespace NebulaModel.Packets.Logistics;

// Sync route update events in GalacticTransport
public class ILSUpdateRoute
{
    public ILSUpdateRoute() { }

    public ILSUpdateRoute(ERouteEvent type, int id0, int id1 = 0, int itemId = 0)
    {
        Type = type;
        Id0 = id0;
        Id1 = id1;
        ItemId = itemId;
    }

    public ERouteEvent Type { get; set; }
    public int Id0 { get; set; }
    public int Id1 { get; set; }
    public int ItemId { get; set; }
    public bool Enable { get; set; }
    public string Comment { get; set; }

    public enum ERouteEvent
    {
        None = 0,
        AddStation2StationRoute,
        RemoveStation2StationRoute_Single,
        RemoveStation2StationRoute_Pair,
        AddAstro2AstroRoute,
        RemoveAstro2AstroRoute,
        SetAstro2AstroRouteEnable,
        SetAstro2AstroRouteComment
    }
}
