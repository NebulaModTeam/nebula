#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SkillSystem))]
internal class SkillSystem_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SkillSystem.AfterTick))]
    public static void AfterTick_Postfix(SkillSystem __instance)
    {
        if (!Multiplayer.IsActive) return;

        // Restore the modified player states
        __instance.CollectPlayerStates();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SkillSystem.MechaEnergyShieldResist),
        [typeof(SkillTarget), typeof(int)],
        [ArgumentType.Normal, ArgumentType.Ref])]
    [HarmonyPatch(nameof(SkillSystem.MechaEnergyShieldResist),
        [typeof(SkillTargetLocal), typeof(int), typeof(int)],
        [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref])]
    public static bool MechaEnergyShieldResist_Prefix(SkillSystem __instance, ref bool __result, ref int damage)
    {
        if (__instance.mecha == GameMain.mainPlayer.mecha) return true;

        damage = 0;
        __result = true;
        return false;
    }
}
