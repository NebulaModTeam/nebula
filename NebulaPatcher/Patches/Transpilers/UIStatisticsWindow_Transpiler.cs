﻿using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(UIStatisticsWindow))]
    public static class UIStatisticsWindow_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIStatisticsWindow.ComputeDisplayEntries))]
        static IEnumerable<CodeInstruction> ComputeDisplayEntries_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceFactoryCondition(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UIStatisticsWindow.ComputePowerTab))]
        static IEnumerable<CodeInstruction> ComputePowerTab_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            /* This is fix for the power statistics.
               Originally, this function is iterating through all factories and manually summing up "energyStored" values from their PowerSystems.
               Since client does not have all factories loaded it would cause exceptions.
             * This fix is basically replacing this:
             
                PowerSystem powerSystem = this.gameData.factories[i].powerSystem;
				int netCursor = powerSystem.netCursor;
				PowerNetwork[] netPool = powerSystem.netPool;
				for (int j = 1; j < netCursor; j++)
				{
					num2 += netPool[j].energyStored;
				}

                With: StatisticsManager.UpdateTotalChargedEnergy(factoryIndex);
                   
             * In the UpdateTotalChargedEnergy(), the total energyStored value is being calculated no clients based on the data received from the server. */
            var matcher = new CodeMatcher(instructions, iLGenerator)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "gameData"),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "factories"),
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(OpCodes.Conv_Ovf_I),
                    new CodeMatch(OpCodes.Ldelem_Ref),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "powerSystem"),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "netCursor"),
                    new CodeMatch(i => i.IsStloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "netPool"),
                    new CodeMatch(i => i.IsStloc()),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(i => i.IsStloc()),
                    new CodeMatch(OpCodes.Br)
                );

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("UIStatisticsWindow_Transpiler.ComputePowerTab_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            int currentPos = matcher.Pos;

            CodeInstruction storeNum2Instruction = matcher.InstructionAt(-1);
            CodeInstruction loadFactoryIndexInstruction = matcher.InstructionAt(3);

            return matcher.MatchForward(true,
                           new CodeMatch(OpCodes.Blt)
                           )
                           .Advance(1)
                           .CreateLabel(out Label endLabel)
                           .Start()
                           .Advance(currentPos)
                           .InsertAndAdvance(loadFactoryIndexInstruction)
                           .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<long, long>>((factoryIndex) =>
                           {
                               return NebulaWorld.Statistics.StatisticsManager.UpdateTotalChargedEnergy((int)factoryIndex);
                           }))
                           .InsertAndAdvance(storeNum2Instruction)
                           .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                           {
                               return SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient;
                           }))
                           .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, endLabel))
                           .InstructionEnumeration();
                           
        }

        public static IEnumerable<CodeInstruction> ReplaceFactoryCondition(IEnumerable<CodeInstruction> instructions)
        {
            //change: if (starData.planets[j].factory != null)
            //to    : if (starData.planets[j].factoryIndex != -1)
            var matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "planets"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Ldelem_Ref),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "factory"),
                    new CodeMatch(OpCodes.Brfalse)
                );

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("UIStatisticsWindow_Transpiler.ReplaceFactoryCondition failed. Mod version not compatible with game version.");
                return instructions;
            }

            return matcher
                    .Advance(-1)
                    .SetOperandAndAdvance(typeof(PlanetData).GetField("factoryIndex", BindingFlags.Public | BindingFlags.Instance))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_M1))
                    .SetInstruction(new CodeInstruction(OpCodes.Beq, matcher.Instruction.operand))
                    .InstructionEnumeration();
        }
    }
}