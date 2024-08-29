using System.Collections.Generic;

namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPFilterResultsResponse
{
    public LCPFilterResultsResponse() { }

    public LCPFilterResultsResponse(List<ControlPanelAstroData> sortedAstros, List<ControlPanelTarget> targets)
    {
        SortedAstroIds = new int[sortedAstros.Count];
        for (var i = 0; i < sortedAstros.Count; i++)
        {
            SortedAstroIds[i] = sortedAstros[i].astroId;
        }

        var resultTargetCount = targets.Count;
        ObjTypes = new short[resultTargetCount];
        ObjIds = new int[resultTargetCount];
        AstroIds = new int[resultTargetCount];
        EntryTypes = new short[resultTargetCount];
        for (var i = 0; i < resultTargetCount; i++)
        {
            var target = targets[i];
            ObjTypes[i] = (short)target.objType;
            ObjIds[i] = target.objId;
            AstroIds[i] = target.astroId;
            EntryTypes[i] = (short)target.entryType;
        }
    }

    public int[] SortedAstroIds { get; set; }
    public short[] ObjTypes { get; set; }
    public int[] ObjIds { get; set; }
    public int[] AstroIds { get; set; }
    public short[] EntryTypes { get; set; }
}
