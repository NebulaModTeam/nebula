#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch]
internal class SkillSystem_Common_Patch
{
    static void SwtichPlayerState(int playerId)
    {
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(playerId, out var index)) return;

        ref var playerData = ref Multiplayer.Session.Combat.Players[index];
        var skillSystem = GameMain.data.spaceSector.skillSystem;
        skillSystem.mecha = playerData.mecha;
        skillSystem.playerSkillTargetL = playerData.skillTargetL;
        skillSystem.playerSkillTargetULast = playerData.skillTargetULast;
        skillSystem.playerSkillTargetU = playerData.skillTargetU;
        skillSystem.playerSkillCastLeftL = playerData.skillTargetL;
        skillSystem.playerSkillCastLeftU = playerData.skillTargetU;
        skillSystem.playerSkillCastRightL = playerData.skillTargetL;
        skillSystem.playerSkillCastRightU = playerData.skillTargetU;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalGeneralProjectile), nameof(LocalGeneralProjectile.TickSkillLogic))]

    public static void LocalGeneralProjectile_Prefix(ref LocalGeneralProjectile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwtichPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalLaserContinuous), nameof(LocalLaserContinuous.TickSkillLogic))]
    public static void LocalLaserContinuous_Prefix(ref LocalLaserContinuous __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwtichPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalLaserOneShot), nameof(LocalLaserOneShot.TickSkillLogic))]
    public static void LocalLaserOneShot_Prefix(ref LocalLaserOneShot __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwtichPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalCannonade), nameof(LocalCannonade.TickSkillLogic))]
    public static void LocalCannonade_Prefix(ref LocalCannonade __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GeneralProjectile), nameof(GeneralProjectile.TickSkillLogic))]
    public static void GeneralProjectile_Prefix(ref GeneralProjectile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwtichPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpaceLaserOneShot), nameof(SpaceLaserOneShot.TickSkillLogic))]
    public static void SpaceLaserOneShot_Prefix(ref SpaceLaserOneShot __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwtichPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpaceLaserSweep), nameof(SpaceLaserSweep.TickSkillLogic))]
    public static void SpaceLaserSweep_Prefix(ref SpaceLaserSweep __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwtichPlayerState(__instance.caster.id);
    }
}
