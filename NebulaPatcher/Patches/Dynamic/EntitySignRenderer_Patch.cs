#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EntitySignRenderer))]
internal class EntitySignRenderer_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EntitySignRenderer.Init))]
    public static void Init_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        EntitySignRenderer.showIcon = Config.Options.BuildingIconEnabled;
        EntitySignRenderer.showSign = Config.Options.BuildingWarningEnabled;
    }
}
