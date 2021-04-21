using HarmonyLib;
using NebulaWorld.Player;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(MechaDroneLogic))]
    class MechaDroneLogic_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("UpdateTargets")]
        static IEnumerable<CodeInstruction> UpdateTargets_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            /*
             * Call DroneManager.BroadcastDroneOrder(int droneId, int entityId, int stage) when drone gets new order
             */
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Pop &&
                    codes[i - 1].opcode == OpCodes.Callvirt &&
                    codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                    codes[i - 2].opcode == OpCodes.Ldloc_3)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 11),
                        new CodeInstruction(OpCodes.Ldloc_3),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), "BroadcastDroneOrder", new System.Type[] { typeof(int), typeof(int), typeof(int) }))
                        });
                    break;
                }
            }

            /*
             * Update search for new targets. Do not include those for whose build request was sent and client is waiting for the response from server
             * Change:
                 if (a.sqrMagnitude > this.sqrMinBuildAlt && sqrMagnitude <= num2 && !this.serving.Contains(num4))
             * 
             * To:
                 if (a.sqrMagnitude > this.sqrMinBuildAlt && sqrMagnitude <= num2 && !this.serving.Contains(num4) && !DroneManager.IsPendingBuildRequest(num4) && DroneManager.AmIClosestPlayer(sqrMagnitude, ref a))
             */
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brtrue &&
                    codes[i - 1].opcode == OpCodes.Callvirt &&
                    codes[i + 1].opcode == OpCodes.Ldloc_S &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 2].opcode == OpCodes.Stloc_2)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 6),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), nameof(DroneManager.IsPendingBuildRequest), new System.Type[] { typeof(int) })),
                        new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand),
                        new CodeInstruction(OpCodes.Ldloca_S, 7),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), nameof(DroneManager.AmIClosestPlayer), new System.Type[] { typeof(Vector3).MakeByRefType() })),
                        new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand),
                        });
                    break;
                }
            }

            return codes;
        }

        /*
         * Call DroneManager.BroadcastDroneOrder(int droneId, int entityId, int stage) when drone's stage changes
         */
        [HarmonyTranspiler]
        [HarmonyPatch("UpdateDrones")]
        static IEnumerable<CodeInstruction> UpdateDrones_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse &&
                    codes[i - 1].opcode == OpCodes.Call &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 1].opcode == OpCodes.Ldloc_0 &&
                    codes[i - 2].opcode == OpCodes.Ldloca_S)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_S, 7),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), "BroadcastDroneOrder", new System.Type[] { typeof(int), typeof(int), typeof(int) }))
                        });
                    break;
                }
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Br &&
                    codes[i - 1].opcode == OpCodes.Pop &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 1].opcode == OpCodes.Ldloc_0 &&
                    codes[i - 2].opcode == OpCodes.Callvirt &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    codes.InsertRange(i + 5, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldelema, typeof(MechaDrone)),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MechaDrone), "targetObject")),
                        new CodeInstruction(OpCodes.Ldc_I4_3),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), "BroadcastDroneOrder", new System.Type[] { typeof(int), typeof(int), typeof(int) }))
                        });
                    break;
                }
            }
            return codes;
        }
    }
}
