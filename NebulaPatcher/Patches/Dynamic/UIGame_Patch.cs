using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGame))]
    class UIGame_Patch
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
    }
}
