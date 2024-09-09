namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPObjectEntryRequest
{
    public LCPObjectEntryRequest() { }

    public void Set(UIControlPanelObjectEntry controlPanelFilter, bool isInit = false)
    {
        // isInit: Request for entityData.protoId, name
        Index = controlPanelFilter.index;
        EntryType = (short)controlPanelFilter.entryType;
        AstroId = controlPanelFilter.target.astroId;
        // Use sign of ObjId to note the state of request
        ObjId = isInit ? -controlPanelFilter.target.objId : controlPanelFilter.target.objId;
    }

    public static readonly LCPObjectEntryRequest Instance = new();

    public int Index { get; set; }
    public short EntryType { get; set; }
    public int AstroId { get; set; }
    public int ObjId { get; set; }
}
