#region

using ABN;
using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameAbnormalityData_0925))]
internal class GameAbnormalityData_0925_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameAbnormalityData_0925.TriggerAbnormality))]
    public static bool TriggerAbnormality_Prefix()
    {
        return !Multiplayer.IsActive || !Config.Options.EnableAchievement;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameAbnormalityData_0925.NothingAbnormal))]
    [HarmonyPatch(nameof(GameAbnormalityData_0925.IsAbnormalTriggerred))]
    public static bool NothingAbnormal_Prefix(ref bool __result)
    {
        if (!Multiplayer.IsActive || !Config.Options.EnableAchievement)
        {
            return true;
        }

        __result = true;
        return false;
    }
}
