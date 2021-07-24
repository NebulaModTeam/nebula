using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(EntitySignRenderer))]
    class EntitySignRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init_Postfix()
        {
            AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showIcon") = Config.Options.BuildingIconEnabled;
            AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showSign") = Config.Options.BuildingWarningEnabled;
        }
    }
}
