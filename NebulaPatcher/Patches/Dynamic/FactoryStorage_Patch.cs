#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(FactoryStorage))]
internal class FactoryStorage_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryStorage.GameTick))]
    public static void GameTick_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FactoryStorage.GameTick))]
    public static void GameTick_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }
    }
}
