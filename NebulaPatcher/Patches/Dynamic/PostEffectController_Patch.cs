#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PostEffectController))]
internal class PostEffectController_Patch
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
