using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Player;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(MechaDroneLogic))]
    class MechaDroneLogic_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch("UpdateTargets")]
        static IEnumerable<CodeInstruction> UpdateTargets_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            /*
             * Update search for new targets. Do not include targets that are already pending response from server.
             * Change:
             *   if (!this.serving.Contains(num4) && (prebuildPool[i].itemRequired == 0 || prebuildPool[i].itemRequired <= this.player.package.GetItemCount((int)prebuildPool[i].protoId)))
             * 
             * To:
             *   if (!this.serving.Contains(num4) && !Multiplayer.Session.Drones.IsPendingBuildRequest(num4) && (prebuildPool[i].itemRequired == 0 || prebuildPool[i].itemRequired <= this.player.package.GetItemCount((int)prebuildPool[i].protoId)))
             */
            var codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(true,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "serving"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "Contains"),
                    new CodeMatch(OpCodes.Brtrue)
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("MechaDroneLogic_Transpiler.UpdateTargets_Transpiler 1 failed. Mod version not compatible with game version.");
                return instructions;
            }

            var num4Instruction = codeMatcher.InstructionAt(-2);
            var jumpInstruction = codeMatcher.Instruction;

            codeMatcher = codeMatcher
                          .Advance(1)
                          .InsertAndAdvance(num4Instruction)
                          .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<int, bool>>((num4) =>
                          {
                              return Multiplayer.Session.Drones.IsPendingBuildRequest(num4);
                          }))
                          .InsertAndAdvance(jumpInstruction);

            /*
             * Make sure targets are only chosen if player is closest to the build preview
             * Change:
             *  if (a.sqrMagnitude > this.sqrMinBuildAlt && sqrMagnitude <= num2)
             * To:
             *  if (Multiplayer.Session.Drones.AmIClosestPlayer(ref a) && a.sqrMagnitude > this.sqrMinBuildAlt && sqrMagnitude <= num2)
            */
            codeMatcher = codeMatcher
                            .MatchForward(false,
                                new CodeMatch(i => i.IsLdloc()),
                                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_sqrMagnitude"),
                                new CodeMatch(OpCodes.Ldarg_0),
                                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "sqrMinBuildAlt"),
                                new CodeMatch(OpCodes.Ble_Un)
                            );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("MechaDroneLogic_Transpiler.UpdateTargets_Transpiler 2 failed. Mod version not compatible with game version.");
                return codeMatcher.InstructionEnumeration();
            }

            var aOperand = codeMatcher.Instruction.operand;
            var jumpOperand = codeMatcher.InstructionAt(4).operand;

            codeMatcher = codeMatcher
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, aOperand))
                            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<Vector3, bool>>((aVar) =>
                            {
                                return Multiplayer.Session.Drones.AmIClosestPlayer(ref aVar);
                            }))
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, jumpOperand));

            /*
             * Insert
             *  Multiplayer.Session.Drones.BroadcastDroneOrder(droneId, entityId, stage);
             * After
             *  this.serving.Add(num3);
            */
            codeMatcher = codeMatcher
                            .MatchForward(true,
                                new CodeMatch(i => i.IsLdarg()),
                                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "serving"),
                                new CodeMatch(i => i.IsLdloc()),
                                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "Add"),
                                new CodeMatch(OpCodes.Pop)
                            );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("MechaDroneLogic_Transpiler.UpdateTargets_Transpiler 3 failed. Mod version not compatible with game version.");
                return codeMatcher.InstructionEnumeration();
            }

            // The index from drones[j]
            var droneIdInstruction = codeMatcher.InstructionAt(-8);

            // num3 from this.serving.Add(num3);
            var entityIdInstruction = codeMatcher.InstructionAt(-2);

            // drones[j].stage = 1;
            var stageInstruction = new CodeInstruction(OpCodes.Ldc_I4_1);

            return codeMatcher
                    .Advance(1)
                    .InsertAndAdvance(droneIdInstruction)
                    .InsertAndAdvance(entityIdInstruction)
                    .InsertAndAdvance(stageInstruction)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int, int, int>>((droneId, entityId, stage) =>
                    {
                        Multiplayer.Session.Drones.BroadcastDroneOrder(droneId, entityId, stage);
                    }))
                    .InstructionEnumeration();
        }

        /*
         * Call Multiplayer.Session.Drones.BroadcastDroneOrder(int droneId, int entityId, int stage) when drone's stage changes
         */
        [HarmonyTranspiler]
        [HarmonyPatch("UpdateDrones")]
        static IEnumerable<CodeInstruction> UpdateDrones_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse &&
                    codes[i - 1].opcode == OpCodes.Call &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 1].opcode == OpCodes.Ldloc_0 &&
                    codes[i - 2].opcode == OpCodes.Ldloca_S)
                {
                    found = true;
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_S, 6),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), "BroadcastDroneOrder", new System.Type[] { typeof(int), typeof(int), typeof(int) }))
                        });
                    break;
                }
            }

            if (!found)
                NebulaModel.Logger.Log.Error("UpdateDrones transpiler 1 failed. Mod version not compatible with game version.");

            found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Br &&
                    codes[i - 1].opcode == OpCodes.Pop &&
                    codes[i + 2].opcode == OpCodes.Ldloc_S &&
                    codes[i + 1].opcode == OpCodes.Ldloc_0 &&
                    codes[i - 2].opcode == OpCodes.Callvirt &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    found = true;
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

            if (!found)
                NebulaModel.Logger.Log.Error("UpdateDrones transpiler 2 failed. Mod version not compatible with game version.");

            return codes;
        }

        /*
         * Changes
         *     if (vector.sqrMagnitude > this.sqrMinBuildAlt && zero2.sqrMagnitude <= num && sqrMagnitude <= num2 && !this.serving.Contains(num4))
         * To
         *     if (vector.sqrMagnitude > this.sqrMinBuildAlt && zero2.sqrMagnitude <= num && sqrMagnitude <= num2 && !this.serving.Contains(num4) && !Multiplayer.Session.Drones.IsPendingBuildRequest(num4))
         * To avoid client's drones from trying to target pending request (caused by drone having additional tasks unlocked via the Communication control tech)
        */
        [HarmonyTranspiler]
        [HarmonyPatch("FindNext")]
        static IEnumerable<CodeInstruction> FindNext_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            var codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "serving"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Brtrue)
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("MechaDroneLogic_Transpiler.FindNext failed. Mod version not compatible with game version.");
                return instructions;
            }

            var target = codeMatcher.InstructionAt(1);
            var jump = codeMatcher.InstructionAt(3).operand;
            return codeMatcher
                   .Advance(4)
                   .InsertAndAdvance(target)
                   .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<int, bool>>((targetId) =>
                   {
                       return Multiplayer.Session.Drones.IsPendingBuildRequest(targetId);
                   }))
                   .Insert(new CodeInstruction(OpCodes.Brtrue, jump))
                   .InstructionEnumeration();
        }
    }
}
