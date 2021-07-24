using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystemRenderer))]
    class PowerSystemRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init_Postfix(UIGameMenu __instance)
        {
            AccessTools.StaticFieldRefAccess<bool>(typeof(PowerSystemRenderer), "powerGraphOn") = Config.Options.PowerGridEnabled;
        }
    }
}
