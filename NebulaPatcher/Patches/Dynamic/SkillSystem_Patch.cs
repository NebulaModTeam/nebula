#region

using System.IO;
using HarmonyLib;
using NebulaModel.Packets.Combat;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SkillSystem))]
internal class SkillSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SkillSystem.Export))]
    public static bool Export_Prefix(SkillSystem __instance, BinaryWriter w)
    {
        if (!NebulaWorld.Combat.CombatManager.SerializeOverwrite) return true;

        w.Write(3); // version 3
        __instance.combatStats.Export(w);
        w.Write(__instance.removedSkillTargets.Count);
        foreach (var skillTarget in __instance.removedSkillTargets)
        {
            w.Write(skillTarget.id);
            w.Write(skillTarget.astroId);
            w.Write((int)skillTarget.type);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SkillSystem.Import))]
    public static bool Import_Prefix(SkillSystem __instance, BinaryReader r)
    {
        if (!NebulaWorld.Combat.CombatManager.SerializeOverwrite) return true;

        _ = r.ReadInt32();
        __instance.combatStats.Import(r);
        var count = r.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            SkillTarget skillTarget;
            skillTarget.id = r.ReadInt32();
            skillTarget.astroId = r.ReadInt32();
            skillTarget.type = (ETargetType)r.ReadInt32();
            __instance.removedSkillTargets.Add(skillTarget);
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SkillSystem.CollectPlayerStates))]
    public static void CollectPlayerStates_Postfix(SkillSystem __instance)
    {
        if (!Multiplayer.IsActive) return;

        // Set those flags to false so AddSpaceEnemyHatred can add threat correctly for client's skill in host
        __instance.playerIsSailing = false;
        __instance.playerIsWarping = false;
        // Set this flag to true so AddSpaceEnemyHatred can add threat correctly from craft/skill of other players even if host is dead (dedicated server)
        __instance.playerAlive = true;
    }

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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SkillSystem.DamageObject))]
    public static void DamageObject_Prefix(int damage, int slice, ref SkillTarget target, ref SkillTarget caster)
    {
        if (!(caster.type == ETargetType.Craft || caster.type == ETargetType.Player)
            || target.type != ETargetType.Enemy
            || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;

        if (target.astroId > 1000000) // Sync for space enemy
        {
            var packet = new CombatStatDamagePacket(damage, slice, in target, in caster)
            {
                // Change the caster to player as craft (space fleet) is not sync yet
                CasterType = (short)ETargetType.Player,
                CasterId = Multiplayer.Session.LocalPlayer.Id
            };
            Multiplayer.Session.Network.SendPacket(packet);
        }
        else if (target.astroId == GameMain.localPlanet?.id) // Sync for local planet
        {
            var packet = new CombatStatDamagePacket(damage, slice, in target, in caster)
            {
                // Change the caster to player as craft (space fleet) is not sync yet
                CasterType = (short)ETargetType.Player,
                CasterId = Multiplayer.Session.LocalPlayer.Id
            };
            Multiplayer.Session.Network.SendPacketToLocalPlanet(packet);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SkillSystem.DamageGroundObjectByLocalCaster))]
    public static void DamageGroundObjectByLocalCaster_Prefix(PlanetFactory factory, int damage, int slice, ref SkillTarget target, ref SkillTarget caster)
    {
        if (caster.type != ETargetType.Craft
            || target.type != ETargetType.Enemy
            || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value) return;

        if (factory == GameMain.localPlanet?.factory) // Sync for local planet combat drones
        {
            target.astroId = caster.astroId = GameMain.localPlanet.astroId;
            var packet = new CombatStatDamagePacket(damage, slice, in target, in caster)
            {
                // Change the caster to player as craft (space fleet) is not sync yet
                CasterType = (short)ETargetType.Player,
                CasterId = Multiplayer.Session.LocalPlayer.Id
            };
            Multiplayer.Session.Network.SendPacketToLocalPlanet(packet);
        }
    }
}
