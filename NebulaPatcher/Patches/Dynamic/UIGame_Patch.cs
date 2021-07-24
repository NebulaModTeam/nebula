using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGame))]
    class UIGame_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("_OnInit")]
        public static void _OnInit_Postfix(UIGame __instance)
        {
            __instance.dfSpaceGuideOn = Config.Options.SpaceNavigationEnabled;
            __instance.dfVeinOn = Config.Options.VeinDistributionEnabled;
        }
    }
}
