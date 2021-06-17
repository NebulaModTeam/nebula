using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(BuildTool_Path))]
    class BuildTool_Path_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch("CreatePrebuilds")]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);

            //Prevent spending items if user is not building
            //insert:  if (!FactoryManager.EventFromServer)
            //before check for resources to build
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand?.ToString() == "Player get_player()" &&
                    codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand?.ToString() == "Int32 get_inhandItemId()" &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 3].opcode == OpCodes.Bne_Un)
                {
                    Label targetLabel = (Label)codes[i + 16].operand;
                    codes.InsertRange(i - 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Stloc, 6),
                        new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), nameof(FactoryManager.EventFromServer))),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "get_Value")),
                        new CodeInstruction(OpCodes.Brtrue_S, targetLabel)
                        });
                    break;
                }
            }

            Label jmpLabel;
            CodeMatcher matcher = new CodeMatcher(codes, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_controller"),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerController), "cmd")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(CommandState), "stage")));
            matcher
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), nameof(FactoryManager.IsHumanInput))));
            matcher.CreateLabelAt(matcher.Pos + 19, out jmpLabel);
            matcher
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, jmpLabel));

            codes = (List<CodeInstruction>)matcher.InstructionEnumeration();

            return codes;
        }


        /* Insert:
         * if (!FactoryManager.IgnoreBasicBuildConditionChecks) {...}
         * - for the inventory check and ground condition check
         * - checks for presence of ore or oil, since we do not want to load colliders for remote planets
         */
        //[HarmonyTranspiler]
        //[HarmonyPatch("CheckBuildConditions")]
        static IEnumerable<CodeInstruction> CheckBuildConditions_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            Label jmpLabel;
            CodeMatcher matcher = new CodeMatcher(instructions, gen)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), "coverObjId")),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), "willRemoveCover")),
                    new CodeMatch(OpCodes.Brfalse))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")));
            matcher.CreateLabelAt(matcher.Pos + 41, out jmpLabel);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue_S, jmpLabel));
            matcher.CreateLabelAt(matcher.Pos - 3, out jmpLabel);
            matcher
                .Advance(-8)
                .Set(OpCodes.Bge_Un_S, jmpLabel);
            return matcher.InstructionEnumeration();
        }
    }
}
