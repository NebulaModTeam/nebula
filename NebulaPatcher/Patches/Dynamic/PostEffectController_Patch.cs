using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PostEffectController))]
    class PostEffectController_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void Start_Postfix(UIGameMenu __instance)
        {
            if (!Multiplayer.IsActive || LocalPlayer.IsMasterClient)
            {
                return;
            }

            AccessTools.StaticFieldRefAccess<bool>(typeof(PostEffectController), "headlight") = Config.Options.GuidingLightEnabled;
        }
    }
}
