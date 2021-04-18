using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
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
                if (codes[i].operand?.ToString() == "System.Int32 coverObjId" &&
                    codes[i + 1].opcode == OpCodes.Brfalse &&
                    codes[i + 3].operand?.ToString() == "System.Boolean willCover" &&
                    codes[i + 4].opcode == OpCodes.Brfalse &&
                    codes[i + 6].operand?.ToString() == "ItemProto item")
                {
                    Label targetLabel = (Label)codes[i + 4].operand;
                    codes.InsertRange(i - 1, new CodeInstruction[] {
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
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand?.ToString() == "System.Boolean willCover" &&
                    codes[i + 1].opcode == OpCodes.Brfalse &&
                    codes[i - 1].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 2].opcode == OpCodes.Brfalse &&
                    codes[i - 3].opcode == OpCodes.Ldfld && codes[i - 3].operand?.ToString() == "System.Int32 coverObjId" &&
                    codes[i - 4].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 5].opcode == OpCodes.Br &&
                    codes[i + 2].opcode == OpCodes.Ldloc_3)
                {
                    Label targetLabel = (Label)codes[i + 1].operand;
                    codes.InsertRange(i - 3, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Pop),
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    new CodeInstruction(OpCodes.Ldloc_3)
                                    });
                    break;
                }
            }

            //Apply Ignoring if inserter match the planet grid check
            for (i = 5; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse &&
                    codes[i + 1].opcode == OpCodes.Ldloca_S &&
                    codes[i - 1].opcode == OpCodes.Ldloc_S &&
                    codes[i - 2].opcode == OpCodes.Stfld &&
                    codes[i - 3].opcode == OpCodes.Call &&
                    codes[i - 4].opcode == OpCodes.Ldfld &&
                    codes[i - 5].opcode == OpCodes.Ldloc_3 &&
                    codes[i + 2].opcode == OpCodes.Ldloc_3)
                {
                    Label targetLabel = (Label)codes[i].operand;
                    codes.InsertRange(i+1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    });
                    break;
                }
            }

            //Apply ignoring of 3 ground checks
            for (i = 9; i < codes.Count - 4; i++)
            {
                if (codes[i - 8].opcode == OpCodes.Blt &&
                    codes[i - 7].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 6].opcode == OpCodes.Ldfld &&
                    codes[i - 5].opcode == OpCodes.Ldfld &&
                    codes[i - 4].opcode == OpCodes.Brfalse &&
                    codes[i - 3].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 2].opcode == OpCodes.Ldfld &&
                    codes[i - 1].opcode == OpCodes.Brfalse &&
                    codes[i].opcode == OpCodes.Br)
                {
                    codes.InsertRange(i - 7, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand)
                                    });

                    Label myCode = gen.DefineLabel();
                    codes[i - 7].labels.Add(myCode);

                    //Go back and fix the jump
                    for (int a = i; a > 0; a--)
                    {
                        if (codes[a].opcode == OpCodes.Brfalse &&
                            codes[a + 1].opcode == OpCodes.Ldarg_0 &&
                            codes[a - 1].opcode == OpCodes.Ldfld &&
                            codes[a + 2].opcode == OpCodes.Ldloca_S &&
                            codes[a - 2].opcode == OpCodes.Ldfld &&
                            codes[a + 3].opcode == OpCodes.Ldfld &&
                            codes[a - 3].opcode == OpCodes.Ldloc_3)
                        {
                            codes[a] = new CodeInstruction(OpCodes.Brfalse, myCode);
                            break;
                        }
                    }

                    break;
                }
            }

            //Apply patch for the ore check
            for (i = 9; i < codes.Count - 4; i++)
            {
                if (codes[i - 8].opcode == OpCodes.Br &&
                    codes[i - 7].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 6].opcode == OpCodes.Ldc_I4_S &&
                    codes[i - 5].opcode == OpCodes.Stfld &&
                    codes[i - 4].opcode == OpCodes.Br &&
                    codes[i - 3].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 2].opcode == OpCodes.Ldfld &&
                    codes[i - 1].opcode == OpCodes.Ldfld &&
                    codes[i].opcode == OpCodes.Brfalse)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand)
                                    });
                    break;
                }
            }

            //Apply patch for the oil check
            for (int a = i; a < codes.Count - 4; a++)
            {
                if (codes[a - 8].opcode == OpCodes.Ldloc_3 &&
                    codes[a - 7].opcode == OpCodes.Ldc_I4_S &&
                    codes[a - 6].opcode == OpCodes.Stfld &&
                    codes[a - 5].opcode == OpCodes.Br &&
                    codes[a - 4].opcode == OpCodes.Br &&
                    codes[a - 3].opcode == OpCodes.Ldloc_3 &&
                    codes[a - 2].opcode == OpCodes.Ldfld &&
                    codes[a - 1].opcode == OpCodes.Ldfld &&
                    codes[a].opcode == OpCodes.Brfalse)
                {
                    codes.InsertRange(a + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[a].operand)
                                    });
                }
            }

            return codes;
        }
    }
}
