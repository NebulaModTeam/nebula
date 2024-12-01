using System;
using NebulaAPI.DataStructures;

namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPFilterResultsRequest
{
    public LCPFilterResultsRequest() { }

    public LCPFilterResultsRequest(in ControlPanelFilter controlPanelFilter)
    {
        TypeFilter = (int)controlPanelFilter.typeFilter;
        AstorFilter = controlPanelFilter.astroFilter;
        if (controlPanelFilter.itemsFilter != null)
        {
            ItemsFilter = new int[controlPanelFilter.itemsFilter.Length];
            Array.Copy(controlPanelFilter.itemsFilter, ItemsFilter, ItemsFilter.Length);
        }
        else
        {
            ItemsFilter = Array.Empty<int>();
        }
        StateFilter = controlPanelFilter.stateFilter;
        SearchFilter = controlPanelFilter.searchFilter;
        SortMethod = (short)controlPanelFilter.sortMethod;

        LocalPlanetAstroId = GameMain.data.localPlanet?.astroId ?? 0;
        LocalStarAstroId = GameMain.data.localStar?.astroId ?? 0;
        PlayerUposition = new Double3(GameMain.mainPlayer.uPosition.x, GameMain.mainPlayer.uPosition.y, GameMain.mainPlayer.uPosition.z);
    }

    public int TypeFilter { get; set; }
    public int AstorFilter { get; set; }
    public int[] ItemsFilter { get; set; }
    public int StateFilter { get; set; }
    public string SearchFilter { get; set; }
    public short SortMethod { get; set; }
    public int LocalPlanetAstroId { get; set; }
    public int LocalStarAstroId { get; set; }
    public Double3 PlayerUposition { get; set; }
}
