using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(BuildTool_Path))]
    class BuildTool_Path_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch("CreatePrebuilds")]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions)
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
            return codes;
        }


        /* Insert:
         * if (!FactoryManager.IgnoreBasicBuildConditionChecks) {...}
         * - for the inventory check and ground condition check
         * - checks for presence of ore or oil, since we do not want to load colliders for remote planets
         */
        [HarmonyTranspiler]
        [HarmonyPatch("CheckBuildConditions")]
        static IEnumerable<CodeInstruction> CheckBuildConditions_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int i = 0;

            //Apply Ignoring of inventory check
            for (i = 5; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand?.ToString() == "System.Boolean willRemoveCover" &&
                    codes[i + 1].opcode == OpCodes.Brfalse &&
                    codes[i - 1].opcode == OpCodes.Ldloc_S &&
                    codes[i - 2].opcode == OpCodes.Brfalse &&
                    codes[i - 3].opcode == OpCodes.Ldfld && codes[i - 3].operand?.ToString() == "System.Int32 coverObjId" &&
                    codes[i - 4].opcode == OpCodes.Ldloc_S &&
                    codes[i - 5].opcode == OpCodes.Br &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S)
                {
                    Label targetLabel = (Label)codes[i + 1].operand;
                    codes.InsertRange(i - 3, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Pop),
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    new CodeInstruction(OpCodes.Ldloc_S, 4)
                                    });
                    break;
                }
            }
            return codes;
        }
    }
}
