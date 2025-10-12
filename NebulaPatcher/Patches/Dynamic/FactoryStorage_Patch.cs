#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(FactoryStorage))]
internal class FactoryStorage_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryStorage.GameTickTank))]
    public static void GameTickTank_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FactoryStorage.GameTickTank))]
    public static void GameTickTank_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }
    }
}
