#region

using HarmonyLib;
using NebulaWorld.Chat;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(IconSet))]
public static class IconSet_Patch
{
    [HarmonyPatch(nameof(IconSet.Create))]
    [HarmonyPostfix]
    public static void Create_Postfix(IconSet __instance)
    {
        ChatSpriteSheetManager.Create(__instance);
    }
}
