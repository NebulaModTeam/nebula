using HarmonyLib;
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
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "get_EventFromServer")),
                        new CodeInstruction(OpCodes.Brtrue_S, targetLabel)
                        });
                    break;
                }
            }
            return codes;
        }


        /* Insert:
         * if (!FactoryManager.IgnoreBasicBuildConditionChecks) {...}
         * for the inventory check and ground condition check
         */
        [HarmonyTranspiler]
        [HarmonyPatch("CheckBuildConditions")]
        static IEnumerable<CodeInstruction> CheckBuildConditions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            //Apply Ignoring of inventory check
            for (int i = 5; i < codes.Count - 2; i++)
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
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "get_IgnoreBasicBuildConditionChecks")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    new CodeInstruction(OpCodes.Ldloc_3)
                                    });
                    break;
                }
            }

            //Apply ignoring of 3 ground checks
            for (int i = 9; i < codes.Count - 4; i++)
            {
                if (codes[i - 7].opcode == OpCodes.Callvirt &&
                    codes[i - 6].opcode == OpCodes.Callvirt &&
                    codes[i - 5].opcode == OpCodes.Sub &&
                    codes[i - 4].opcode == OpCodes.Ldc_R4 &&
                    codes[i - 3].opcode == OpCodes.Bge_Un &&
                    codes[i - 2].opcode == OpCodes.Ldloc_3 &&
                    codes[i - 1].opcode == OpCodes.Ldc_I4_S &&
                    codes[i].opcode == OpCodes.Stfld && codes[i].operand?.ToString() == "EBuildCondition condition" &&
                    codes[i + 1].opcode == OpCodes.Br)
                {
                    int numOfPathes = 3;
                    for (int a = i; a < i + 100; a++)
                    {

                        if (codes[a].opcode == OpCodes.Stfld &&
                            codes[a].operand?.ToString() == "EBuildCondition condition" &&
                            codes[a + 1].opcode == OpCodes.Br)
                        {
                            codes.InsertRange(a - 2, new CodeInstruction[] {
                                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "get_IgnoreBasicBuildConditionChecks")),
                                new CodeInstruction(OpCodes.Brtrue_S, codes[a-3].operand),
                            });

                            if (codes[a - 3].opcode == OpCodes.Br)
                            {
                                Label tmp = codes[a - 3].labels[0];
                                codes[a - 3] = new CodeInstruction(OpCodes.Nop);
                                codes[a - 3].labels.Add(tmp);
                            }

                            a += 5;
                            numOfPathes--;
                            UnityEngine.Debug.Log($"{codes[a].opcode} - {codes[a].operand}");
                            if (numOfPathes <= 0)
                            {
                                break;
                            }
                        }

                    }
                    break;
                }
            }

            return codes;
        }
    }
}
