#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIGame))]
internal class UIGame_Patch
{
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
