#region

using HarmonyLib;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIProductEntry))]
internal class UIProductEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIProductEntry.OnRefreshRefSpeedClick))]
    [HarmonyPatch(nameof(UIProductEntry.OnRefreshStorageCountClick))]
    public static void OnRefreshClick_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        // When client left-click the refresh button, request the extra data from server
        var astroFilter = UIRoot.instance.uiGame.statWindow.astroFilter;
        if (astroFilter == 0)
        {
            if (GameMain.localPlanet != null) return;
            astroFilter = GameMain.localStar?.id ?? 0;
        }
        else if (GameMain.localPlanet != null && GameMain.localPlanet.astroId == astroFilter)
        {
            return; // For local planet, use the local calculation
        }
        Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.AstroFilterChanged, astroFilter));
    }
}
