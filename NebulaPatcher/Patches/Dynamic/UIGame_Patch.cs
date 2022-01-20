using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGame))]
    internal class UIGame_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGame._OnInit))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
        public static bool StarmapChangingToMilkyWay_Prefix(UIGame __instance)
        {
            if (Multiplayer.IsActive)
            {
                InGamePopup.ShowInfo("Access Denied", "Milky Way is disabled in multiplayer game.", "OK");
                return false;
            }
            return true;
        }
    }
}
