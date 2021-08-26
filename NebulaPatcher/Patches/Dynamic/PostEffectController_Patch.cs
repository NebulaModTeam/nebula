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
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            PostEffectController.headlight = Config.Options.GuidingLightEnabled;
        }
    }
}
