#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlayerControlGizmo))]
public class PlayerControlGizmo_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerControlGizmo.GameTick))]
    public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControlGizmo), "player")),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_controller"),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_modelVisible"),
                new CodeMatch(OpCodes.Brfalse));
        var jmpPos = matcher.Operand;
        matcher.Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(_this =>
            {
                return Multiplayer.IsActive && _this.player.navigation.indicatorAstroId > 100000;
            }))
            .Insert(new CodeInstruction(OpCodes.Brtrue, jmpPos));

        matcher.MatchForward(true,
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControlGizmo), "player")),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_navigation"),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_indicatorAstroId"),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Ble));
        jmpPos = matcher.Operand;
        matcher.Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(_this =>
            {
                return Multiplayer.IsActive && _this.player.navigation.indicatorAstroId > 100000;
            }))
            .Insert(new CodeInstruction(OpCodes.Brtrue, jmpPos));

        matcher.MatchForward(true,
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControlGizmo), "player")),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_navigation"),
            new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_indicatorAstroId"),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Ble));
        jmpPos = matcher.Operand;
        matcher.Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(_this =>
            {
                return Multiplayer.IsActive && _this.player.navigation.indicatorAstroId > 100000;
            }))
            .Insert(new CodeInstruction(OpCodes.Brtrue, jmpPos));

        return matcher.InstructionEnumeration();
    }
}
