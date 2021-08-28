using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch]
    class BuildTool_Click_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            var codeMatcher = new CodeMatcher(instructions, iL)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Ldsfld && ((FieldInfo)i.operand).Name == "buildTargetAutoMove")
                    );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildTool_Click.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            var label = codeMatcher.InstructionAt(1).operand;
            return codeMatcher
                    .Advance(2)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest && Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id;
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label))
                    .InstructionEnumeration();
        }
    }
}
