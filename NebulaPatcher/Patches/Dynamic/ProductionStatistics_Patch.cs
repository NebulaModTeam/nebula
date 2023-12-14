#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(ProductionStatistics))]
internal class ProductionStatistics_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ProductionStatistics.PrepareTick))]
    public static bool PrepareTick_Prefix(ProductionStatistics __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }
        for (var i = 0; i < __instance.gameData.factoryCount; i++)
        {
            __instance.factoryStatPool[i]?.PrepareTick();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ProductionStatistics.AfterTick))]
    public static bool AfterTick_Prefix(ProductionStatistics __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }
        for (var i = 0; i < __instance.gameData.factoryCount; i++)
        {
            __instance.factoryStatPool[i]?.AfterTick();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ProductionStatistics.GameTick))]
    public static bool GameTick_Prefix(ProductionStatistics __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }
        //Do not run on client if you do not have all data
        for (var i = 0; i < __instance.gameData.factoryCount; i++)
        {
            if (__instance.factoryStatPool[i] == null)
            {
                return false;
            }
        }
        return true;
    }
}
