using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGame))]
    class UIGame_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("_OnInit")]
        public static void _OnInit_Postfix(UIGame __instance)
        {
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            __instance.dfSpaceGuideOn = Config.Options.SpaceNavigationEnabled;
            __instance.dfVeinOn = Config.Options.VeinDistributionEnabled;
        }
    }
}
