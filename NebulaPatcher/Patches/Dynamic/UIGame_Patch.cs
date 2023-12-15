#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIGame))]
internal class UIGame_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGame._OnInit))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnInit_Postfix(UIGame __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        __instance.dfSpaceGuideOn = Config.Options.SpaceNavigationEnabled;
        __instance.dfVeinOn = Config.Options.VeinDistributionEnabled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIGame.StarmapChangingToMilkyWay))]
    public static bool StarmapChangingToMilkyWay_Prefix()
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        InGamePopup.ShowInfo("Unavailable".Translate(), "Milky Way is disabled in multiplayer game.".Translate(),
            "OK".Translate());
        return false;
    }
}
