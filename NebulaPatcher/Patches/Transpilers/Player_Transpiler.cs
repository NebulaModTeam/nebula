﻿using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(Player))]
    public class Player_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Player.nearestFactory), MethodType.Getter)]
        public static IEnumerable<CodeInstruction> Get_nearestFactory_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelem_Ref),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Brfalse));

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("Player.Get_nearestFactory_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            var op = matcher.InstructionAt(5).operand;

            return matcher
                .Advance(-1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                {
                    return LocalPlayer.IsMasterClient || !SimulatedWorld.Initialized;
                }))
                .Insert(new CodeInstruction(OpCodes.Brfalse, op))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Player.Free))]
        public static IEnumerable<CodeInstruction> Free_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_controller"),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "op_Inequality"),
                    new CodeMatch(OpCodes.Brfalse)
                );

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("Player.Free_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            var jumpOperand = matcher.InstructionAt(4).operand;

            return matcher
                .SetOperandAndAdvance(jumpOperand)
                .InstructionEnumeration();
        }
    }
}
