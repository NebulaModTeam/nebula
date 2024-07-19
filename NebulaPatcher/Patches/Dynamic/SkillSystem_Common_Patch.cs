#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch]
internal class SkillSystem_Common_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Bomb_Explosive), nameof(Bomb_Explosive.TickSkillLogic))]
    [HarmonyPatch(typeof(Bomb_Liquid), nameof(Bomb_Liquid.TickSkillLogic))]
    [HarmonyPatch(typeof(Bomb_EMCapsule), nameof(Bomb_EMCapsule.TickSkillLogic))]
    public static void Bomb_TickSkillLogic(ref int ___nearPlanetAstroId, ref int ___life)
    {
        if (___nearPlanetAstroId > 0 && GameMain.spaceSector.skillSystem.astroFactories[___nearPlanetAstroId] == null)
        {
            // The nearest planetFactory hasn't loaded yet, skip and remove
            ___life = 0;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GeneralShieldBurst), nameof(GeneralShieldBurst.TickSkillLogic))]
    public static bool GeneralShieldBurst_Prefix(ref GeneralShieldBurst __instance, SkillSystem skillSystem)
    {
        if (!Multiplayer.IsActive || __instance.caster.type != ETargetType.Player) return true;
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(__instance.caster.id, out var index)) return true;

        // Overwrite the original function
        ref var playerData = ref Multiplayer.Session.Combat.Players[index];
        __instance.upos = playerData.mecha.skillShieldBurstUCenter;
        __instance.rpos = playerData.mecha.skillShieldBurstLCenter;
        if ((__instance.lifeMax - __instance.life) % __instance.damageInterval == 0)
        {
            __instance.DoRangeDamage(skillSystem);
        }
        if (__instance.life > 0)
        {
            __instance.life--;
        }
        return false;
    }

    static void SwitchPlayerState(int playerId)
    {
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(playerId, out var index)) return;

        ref var playerData = ref Multiplayer.Session.Combat.Players[index];
        var skillSystem = GameMain.data.spaceSector.skillSystem;
        skillSystem.mecha = playerData.mecha;
        skillSystem.playerAlive = playerData.isAlive;
        skillSystem.playerSkillTargetL = playerData.skillTargetL;
        skillSystem.playerSkillTargetULast = playerData.skillTargetULast;
        skillSystem.playerSkillTargetU = playerData.skillTargetU;
        skillSystem.playerSkillCastLeftL = playerData.skillTargetL;
        skillSystem.playerSkillCastLeftU = playerData.skillTargetU;
        skillSystem.playerSkillCastRightL = playerData.skillTargetL;
        skillSystem.playerSkillCastRightU = playerData.skillTargetU;
    }

    static void SwitchTargetPlayerWithCollider(int playerId)
    {
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(playerId, out var index)) return;

        ref var playerData = ref Multiplayer.Session.Combat.Players[index];
        var skillSystem = GameMain.data.spaceSector.skillSystem;
        skillSystem.mecha = playerData.mecha;
        skillSystem.playerAlive = playerData.isAlive;
        skillSystem.playerSkillTargetL = playerData.skillTargetL;
        skillSystem.playerSkillTargetULast = playerData.skillTargetULast;
        skillSystem.playerSkillTargetU = playerData.skillTargetU;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GeneralExpImpProjectile), nameof(GeneralExpImpProjectile.TickSkillLogic))]
    public static void GeneralExpImpProjectile_Prefix(ref GeneralExpImpProjectile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchTargetPlayerWithCollider(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GeneralMissile), nameof(GeneralMissile.TickSkillLogic))]
    public static void GeneralProjectile_Prefix(ref GeneralMissile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchTargetPlayerWithCollider(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GeneralProjectile), nameof(GeneralProjectile.TickSkillLogic))]
    public static void GeneralProjectile_Prefix(ref GeneralProjectile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchTargetPlayerWithCollider(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalGeneralProjectile), nameof(LocalGeneralProjectile.TickSkillLogic))]
    public static void LocalGeneralProjectile_Prefix(ref LocalGeneralProjectile __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchTargetPlayerWithCollider(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalLaserContinuous), nameof(LocalLaserContinuous.TickSkillLogic))]
    public static void LocalLaserContinuous_Prefix(ref LocalLaserContinuous __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalLaserOneShot), nameof(LocalLaserOneShot.TickSkillLogic))]
    public static void LocalLaserOneShot_Prefix(ref LocalLaserOneShot __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocalCannonade), nameof(LocalCannonade.TickSkillLogic))]
    public static void LocalCannonade_Prefix(ref LocalCannonade __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpaceLaserOneShot), nameof(SpaceLaserOneShot.TickSkillLogic))]
    public static void SpaceLaserOneShot_Prefix(ref SpaceLaserOneShot __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if (__instance.target.type == ETargetType.Player) SwitchPlayerState(__instance.target.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SpaceLaserSweep), nameof(SpaceLaserSweep.TickSkillLogic))]
    public static void SpaceLaserSweep_Prefix(ref SpaceLaserSweep __instance)
    {
        if (!Multiplayer.IsActive) return;
        if (__instance.caster.type == ETargetType.Player) SwitchPlayerState(__instance.caster.id);
        if ((__instance.mask & ETargetTypeMask.Player) != 0) SwitchTargetPlayerWithCollider(Multiplayer.Session.LocalPlayer.Id);
    }
}
