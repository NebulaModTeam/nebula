using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PostEffectController))]
    class PostEffectController_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PostEffectController.Start))]
        public static void Start_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            PostEffectController.headlight = Config.Options.GuidingLightEnabled;
        }
    }
}
