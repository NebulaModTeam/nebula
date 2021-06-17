using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
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
    }
}
