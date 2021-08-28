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
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }

            PostEffectController.headlight = Config.Options.GuidingLightEnabled;
        }
    }
}
