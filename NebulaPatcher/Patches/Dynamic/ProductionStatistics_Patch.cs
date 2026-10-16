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
    [HarmonyPatch(nameof(ProductionStatistics.GameTick))]
    [HarmonyPatch(nameof(ProductionStatistics.GameTick_Parallel))]
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

    /// <summary>
    /// Clients do not simulate remote factories, so the vanilla power statistics derivation
    /// (<c>RefreshPowerGenerationCapacites</c> / <c>RefreshPowerConsumptionDemands</c>) would
    /// dereference null <c>PlanetFactory.powerSystem</c> instances and throw.
    ///
    /// The host runs this vanilla code path and streams the resulting values through
    /// <c>StatisticsPowerDataPacket</c>, so clients skip the local recompute and keep the
    /// values they received. This makes the Power Dashboard render exactly as in singleplayer.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ProductionStatistics.RefreshPowerGenerationCapacites))]
    [HarmonyPatch(nameof(ProductionStatistics.RefreshPowerConsumptionDemands))]
    [HarmonyPatch(nameof(ProductionStatistics.RefreshPowerNetworkGenerationCapacites))]
    [HarmonyPatch(nameof(ProductionStatistics.RefreshPowerNetworkConsumptionDemands))]
    public static bool RefreshPowerData_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
    }
}
