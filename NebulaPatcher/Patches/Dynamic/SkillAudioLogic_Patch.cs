#region

using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Combat;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SkillAudioLogic))]
internal class SkillAudioLogic_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SkillAudioLogic), nameof(SkillAudioLogic.AddPlayerAudio))]
    public static void AddPlayerAudio_Prefix(ref SkillSFXHolder __result)
    {
        if (!Multiplayer.IsActive) return;
        if (CombatManager.PlayerId == Multiplayer.Session.LocalPlayer.Id) return;

        // Remote player: Set multiplier by distance from the main player
        ref var ptr = ref Multiplayer.Session.Combat.Players[0]; // main player
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(CombatManager.PlayerId, out var index)) return;

        if (ptr.planetId > 0 && ptr.planetId == GameMain.spaceSector.skillSystem.localPlanetOrStarAstroId)
        {
            var dist = (ptr.position - Multiplayer.Session.Combat.Players[index].position).magnitude;
            __result.multiplier = (120f - dist) / 120f;
            if (__result.multiplier < 0)
            {
                __result.multiplier = 0;
            }
        }
        else
        {
            // Mute attacks in space
            __result.multiplier = 0;
        }
    }
}
