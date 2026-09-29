#region

using System;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIGlobemap))]
internal class UIGlobemap_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGlobemap._OnOpen))]
    public static void _OnOpen_Postfix()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        try
        {
            // Rebuild markerCursor and recycle list so MarkerRenderer and MarkerUIRenderer display beacons
            GameMain.data?.galacticDigital?.Arragement();

            // Refresh marker details (floating title & todo text) for local planet
            var uiGame = UIRoot.instance?.uiGame;
            if (uiGame?.markerDetail != null && GameMain.localPlanet != null && GameMain.localPlanet.factory != null)
            {
                uiGame.markerDetail.inspectPlanet = null;
                uiGame.markerDetail.SetInspectPlanet(GameMain.localPlanet);
                uiGame.markerDetail.UpdateNodes();
            }
        }
        catch (Exception e)
        {
            Log.Warn($"UIGlobemap._OnOpen_Postfix error: {e}");
        }
    }
}
