#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlayerAction_Combat))]
internal class PlayerAction_Combat_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShootTarget))]
    public static IEnumerable<CodeInstruction> ShootTarget_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite casterId in AddGroundEnemyHatred and AddSpaceEnemyHatred to playerId
            from:
                this.skillSystem.AddGroundEnemyHatred(dfgbaseComponent, ref ptr, ETargetType.Player, 1);
            to:
                this.skillSystem.AddGroundEnemyHatred(dfgbaseComponent, ref ptr, ETargetType.Player, NebulaWorld.Combat.CombatManager.PlayerId);
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(false,
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddSpaceEnemyHatred"))
                .Set(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(NebulaWorld.Combat.CombatManager),
                    nameof(NebulaWorld.Combat.CombatManager.PlayerId)))
                .MatchBack(false,
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddGroundEnemyHatred"))
                .Set(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(NebulaWorld.Combat.CombatManager),
                    nameof(NebulaWorld.Combat.CombatManager.PlayerId)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler PlayerAction_Combat.Shoot failed.");
            Log.Error(e);
            return instructions;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerAction_Combat.Bombing))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Gauss_Local))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Cannon_Local))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Plasma))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Missile))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Gauss_Space))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Cannon_Space))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Laser_Local))]
    [HarmonyPatch(nameof(PlayerAction_Combat.Shoot_Laser_Space))]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShieldBurst))]
    public static IEnumerable<CodeInstruction> ReplacePlayerId_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite caster id to playerId
            from:
                ptr.caster.id = 1;
            to:
                ptr.caster.id = NebulaWorld.Combat.CombatManager.PlayerId;
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldflda && ((FieldInfo)i.operand).Name == "caster"),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "id"))
                .Repeat(matcher => matcher
                    .Advance(-1)
                    .Set(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(NebulaWorld.Combat.CombatManager),
                        nameof(NebulaWorld.Combat.CombatManager.PlayerId)))
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler PlayerAction_Combat.Shoot failed.");
            Log.Error(e);
            return instructions;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerAction_Combat.Bombing))]
    public static IEnumerable<CodeInstruction> Bombing_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            //  Broadcast bombing event to other players by replacing ptr.ApplyConfigs();

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Bomb_Liquid), nameof(Bomb_Liquid.ApplyConfigs))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .Advance(1)
                .SetOperandAndAdvance(AccessTools.Method(typeof(PlayerAction_Combat_Transpiler), nameof(SendBomb_Liquid)))
                .MatchForward(true, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Bomb_Explosive), nameof(Bomb_Explosive.ApplyConfigs))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .Advance(1)
                .SetOperandAndAdvance(AccessTools.Method(typeof(PlayerAction_Combat_Transpiler), nameof(SendBomb_Explosive)))
                .MatchForward(true, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Bomb_EMCapsule), nameof(Bomb_EMCapsule.ApplyConfigs))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .Advance(1)
                .SetOperandAndAdvance(AccessTools.Method(typeof(PlayerAction_Combat_Transpiler), nameof(SendBomb_EMCapsule)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler PlayerAction_Combat.Bombing failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static void SendBomb_Liquid(ref Bomb_Liquid ptr, PlayerAction_Combat combat)
    {
        ptr.ApplyConfigs();
        if (!Multiplayer.IsActive) return;

        var packet = new MechaBombPacket(
            Multiplayer.Session.LocalPlayer.Id,
            ptr.nearStarId,
            in combat.player.uVelocity,
            in ptr.uVel,
            in ptr.uAgl,
            ptr.protoId);

        Multiplayer.Session.Network.SendPacket(packet);
    }

    static void SendBomb_Explosive(ref Bomb_Explosive ptr, PlayerAction_Combat combat)
    {
        ptr.ApplyConfigs();
        if (!Multiplayer.IsActive) return;

        var packet = new MechaBombPacket(
            Multiplayer.Session.LocalPlayer.Id,
            ptr.nearStarId,
            in combat.player.uVelocity,
            in ptr.uVel,
            in ptr.uAgl,
            ptr.protoId);

        Multiplayer.Session.Network.SendPacket(packet);
    }

    static void SendBomb_EMCapsule(ref Bomb_EMCapsule ptr, PlayerAction_Combat combat)
    {
        ptr.ApplyConfigs();
        if (!Multiplayer.IsActive) return;

        var packet = new MechaBombPacket(
            Multiplayer.Session.LocalPlayer.Id,
            ptr.nearStarId,
            in combat.player.uVelocity,
            in ptr.uVel,
            in ptr.uAgl,
            ptr.protoId);

        Multiplayer.Session.Network.SendPacket(packet);
    }
}
