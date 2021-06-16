using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(BuildTool_Click))]
    class BuildTool_Click_Transpiler
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
                    codes[i - 5].opcode == OpCodes.Stfld &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S)
                {
                    Label targetLabel = (Label)codes[i + 1].operand;
                    codes.InsertRange(i - 3, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Pop),
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    new CodeInstruction(OpCodes.Ldloc_S, 6)
                                    });
                    break;
                }
            }

            //Apply Ignoring if inserter match the planet grid check
            for (i = 5; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse &&
                    codes[i + 1].opcode == OpCodes.Ldloc_S &&
                    codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 2].operand?.ToString() == "EBuildCondition condition" &&
                    codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 1].operand?.ToString() == "System.Boolean isInserter" &&
                    codes[i - 2].opcode == OpCodes.Ldfld &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S &&
                    codes[i - 4].opcode == OpCodes.Blt &&
                    codes[i - 5].opcode == OpCodes.Conv_I4)
                {
                    Label targetLabel = (Label)codes[i].operand;
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    });
                    break;
                }
            }

            //Apply ignoring of 3 ground checks
            for (i = 12; i < codes.Count - 4; i++)
            {
                if (
                    codes[i - 11].opcode == OpCodes.Blt &&
                    codes[i - 10].opcode == OpCodes.Ldloc_S &&
                    codes[i - 9].opcode == OpCodes.Ldfld &&
                    codes[i - 8].opcode == OpCodes.Ldfld && codes[i - 8].operand?.ToString() == "System.Boolean multiLevel" &&
                    codes[i - 7].opcode == OpCodes.Brfalse &&
                    codes[i - 6].opcode == OpCodes.Ldloc_S &&
                    codes[i - 5].opcode == OpCodes.Ldfld && codes[i - 5].operand?.ToString() == "System.Int32 inputObjId" &&
                    codes[i - 4].opcode == OpCodes.Brtrue &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S &&
                    codes[i - 2].opcode == OpCodes.Ldfld &&
                    codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 1].operand?.ToString() == "System.Boolean isInserter" &&
                    codes[i].opcode == OpCodes.Brtrue)
                {
                    codes.InsertRange(i - 10, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand)
                                    });

                    Label myCode = gen.DefineLabel();
                    codes[i - 10].labels.Add(myCode);

                    //Go back and fix the jump and also patch isEjector
                    for (int a = i; a > 0; a--)
                    {
                        if (codes[a].opcode == OpCodes.Brfalse &&
                            codes[a + 1].opcode == OpCodes.Ldarg_0 &&
                            codes[a - 1].opcode == OpCodes.Ldfld && codes[a - 1].operand?.ToString() == "System.Boolean isEjector" &&
                            codes[a + 2].opcode == OpCodes.Ldloc_S &&
                            codes[a - 2].opcode == OpCodes.Ldfld &&
                            codes[a + 3].opcode == OpCodes.Ldc_R4 &&
                            codes[a - 3].opcode == OpCodes.Ldloc_S)
                        {
                            codes[a] = new CodeInstruction(OpCodes.Brfalse, myCode);
                            codes.InsertRange(a + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, myCode)
                                    });
                            break;
                        }
                    }

                    break;
                }
            }

            //Apply patch for the ore check
            for (i = 9; i < codes.Count - 4; i++)
            {
                if (codes[i - 8].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 7].opcode == OpCodes.Ceq &&
                    codes[i - 6].opcode == OpCodes.Br &&
                    codes[i - 5].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 4].opcode == OpCodes.Stloc_S &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S &&
                    codes[i - 2].opcode == OpCodes.Ldfld &&
                    codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 1].operand?.ToString() == "System.Boolean veinMiner" &&
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
                if (codes[a - 8].opcode == OpCodes.Brtrue &&
                    codes[a - 7].opcode == OpCodes.Ldloc_S &&
                    codes[a - 6].opcode == OpCodes.Ldc_I4_S &&
                    codes[a - 5].opcode == OpCodes.Stfld &&
                    codes[a - 4].opcode == OpCodes.Br &&
                    codes[a - 3].opcode == OpCodes.Ldloc_S &&
                    codes[a - 2].opcode == OpCodes.Ldfld &&
                    codes[a - 1].opcode == OpCodes.Ldfld && codes[a - 1].operand?.ToString() == "System.Boolean oilMiner" &&
                    codes[a].opcode == OpCodes.Brfalse)
                {
                    codes.InsertRange(a + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[a].operand)
                                    });
                    break;
                }
            }

            //Apply patch for ejector
            for (i = 0; i < codes.Count - 16; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand?.ToString() == "System.Boolean isInserter" &&
                    codes[i + 3].opcode == OpCodes.Call && codes[i + 3].operand?.ToString() == "Single get_magnitude()" &&
                    codes[i + 6].opcode == OpCodes.Callvirt && codes[i + 6].operand?.ToString() == "Single get_realRadius()" &&
                    codes[i + 10].opcode == OpCodes.Ldfld && codes[i + 10].operand?.ToString() == "System.Single cullingHeight" &&
                    codes[i + 16].opcode == OpCodes.Ldfld && codes[i + 16].operand?.ToString() == "System.Boolean isEjector")
                {
                    codes.InsertRange(i + 2, new CodeInstruction[]
                    {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i + 1].operand),
                    });
                    break;
                }
            }

            //Apply patch for orbital collectors
            for (i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand?.ToString() == "System.Boolean isCollectStation" &&
                    codes[i + 3].opcode == OpCodes.Ldfld && codes[i + 3].operand?.ToString() == "PlanetData planet")
                {
                    codes.InsertRange(i + 2, new CodeInstruction[]
                    {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i + 1].operand),
                    });
                    break;
                }
            }
            return codes;
        }
    }
}
