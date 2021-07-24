using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PostEffectController))]
    class PostEffectController_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Start_Postfix(UIGameMenu __instance)
        {
            AccessTools.StaticFieldRefAccess<bool>(typeof(PostEffectController), "headlight") = Config.Options.GuidingLightEnabled;
        }
    }
}
