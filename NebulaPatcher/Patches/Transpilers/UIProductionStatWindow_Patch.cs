using HarmonyLib;
using NebulaWorld.Statistics;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(UIProductionStatWindow))]
    public static class UIProductionStatWindow_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("ComputeDisplayEntries")]
        static IEnumerable<CodeInstruction> ComputeDisplayEntries_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceFactoryCondition(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("UpdateProduct")]
        static IEnumerable<CodeInstruction> UpdateProduct_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceFactoryCondition(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("UpdateResearch")]
        static IEnumerable<CodeInstruction> UpdateResearch_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceFactoryCondition(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("UpdatePower")]
        static IEnumerable<CodeInstruction> UpdatePower_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /* This is fix for the power statistics.
               Originally, this function is iterating through all factories and manually summing up "energyStored" values from their PowerSystems.
               Since client does not have all factories loaded it would cause exceptions.
             * This fix is basically replacing this on 4 different places:
             
                PowerSystem powerSystem = this.gameData.factories[i].powerSystem;
				int netCursor = powerSystem.netCursor;
				PowerNetwork[] netPool = powerSystem.netPool;
				for (int j = 1; j < netCursor; j++)
				{
					num2 += netPool[j].energyStored;
				}

                With: StatisticsManager.UpdateTotalChargedEnergy(ref num2, targetIndex);
                   
             * In the UpdateTotalChargedEnergy(), the total energyStored value is being calculated no clients based on the data received from the server. */
            bool[] patchActive = { false, false, false, false };
            int[] patchLength = { 30, 33, 33, 27 };
            var targetIndex = typeof(UIProductionStatWindow).GetField("targetIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                //Patch fix for the "Entire Star Cluster" Tab in statistics
                if (codes[i].operand?.ToString() == "PlanetFactory[] factories" && !patchActive[0])
                {
                    patchActive[0] = true;
                    for (int j = 0; j < patchLength[0]; j++)
                    {
                        codes[i - 1 + j].opcode = OpCodes.Nop;
                    }
                    codes[i] = new CodeInstruction(OpCodes.Ldloca_S, 2); //Loads the address of the local variable at a specific index onto the evaluation stack, short form.
                    codes[i + 1] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes[i + 2] = new CodeInstruction(OpCodes.Ldfld, targetIndex);
                    codes[i + 3] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatisticsManager), "UpdateTotalChargedEnergy"));
                }

                //Patch fix for the "Local Planet" tab in statistics
                if (codes[i].operand?.ToString() == "PowerSystem powerSystem" && patchActive[0] && !patchActive[1])
                {
                    patchActive[1] = true;
                    for (int j = 0; j < patchLength[1]; j++)
                    {
                        codes[i - 2 + j].opcode = OpCodes.Nop;
                    }
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4_0);
                    codes[i + 1] = new CodeInstruction(OpCodes.Conv_I8);
                    codes[i + 2] = new CodeInstruction(OpCodes.Stloc_S, 28);
                    codes[i + 3] = new CodeInstruction(OpCodes.Ldloca_S, 28);
                    codes[i + 4] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes[i + 5] = new CodeInstruction(OpCodes.Ldfld, targetIndex);
                    codes[i + 6] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatisticsManager), "UpdateTotalChargedEnergy"));
                    codes[i + 7] = new CodeInstruction(OpCodes.Ldloc_S, 21);
                    codes[i + 8] = new CodeInstruction(OpCodes.Ldfld, typeof(FactoryProductionStat).GetField("energyConsumption", BindingFlags.Public | BindingFlags.Instance));
                    codes[i + 9] = new CodeInstruction(OpCodes.Stloc_S, 29);
                }

                //Patch fix for the "Picking specific planet" in statistics
                if (codes[i].operand?.ToString() == "PowerSystem powerSystem" && patchActive[1] && !patchActive[2])
                {
                    patchActive[2] = true;
                    for (int j = 0; j < patchLength[2]; j++)
                    {
                        codes[i - 2 + j].opcode = OpCodes.Nop;
                    }
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4_0);
                    codes[i + 1] = new CodeInstruction(OpCodes.Conv_I8);
                    codes[i + 2] = new CodeInstruction(OpCodes.Stloc_S, 40);
                    codes[i + 3] = new CodeInstruction(OpCodes.Ldloca_S, 40);
                    codes[i + 4] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes[i + 5] = new CodeInstruction(OpCodes.Ldfld, targetIndex);
                    codes[i + 6] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatisticsManager), "UpdateTotalChargedEnergy"));
                    codes[i + 7] = new CodeInstruction(OpCodes.Ldloc_S, 33);
                    codes[i + 8] = new CodeInstruction(OpCodes.Ldfld, typeof(FactoryProductionStat).GetField("energyConsumption", BindingFlags.Public | BindingFlags.Instance));
                    codes[i + 9] = new CodeInstruction(OpCodes.Stloc_S, 41);
                }

                //Patch fix for the "Picking specific star system" in statistics
                if (codes[i].operand?.ToString() == "PowerSystem powerSystem" && patchActive[2] && !patchActive[3])
                {
                    patchActive[3] = true;
                    for (int j = 0; j < patchLength[3]; j++)
                    {
                        codes[i - 2 + j].opcode = OpCodes.Nop;
                    }
                    codes[i] = new CodeInstruction(OpCodes.Ldloca_S, 47); //Loads the address of the local variable at a specific index onto the evaluation stack, short form.
                    codes[i + 1] = new CodeInstruction(OpCodes.Ldarg_0);
                    codes[i + 2] = new CodeInstruction(OpCodes.Ldfld, targetIndex);
                    codes[i + 3] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatisticsManager), "UpdateTotalChargedEnergy"));
                }
            }
            return ReplaceFactoryCondition(codes);
        }

        public static IEnumerable<CodeInstruction> ReplaceFactoryCondition(IEnumerable<CodeInstruction> instructions)
        {
            //change: if (starData.planets[j].factory != null)
            //to    : if (starData.planets[j].factoryIndex != -1)
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand?.ToString() == "PlanetFactory factory" && (codes[i + 1].opcode == OpCodes.Brtrue || codes[i + 1].opcode == OpCodes.Brfalse))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldfld, typeof(PlanetData).GetField("factoryIndex", BindingFlags.Public | BindingFlags.Instance));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4_M1));
                    if (codes[i + 2].opcode == OpCodes.Brtrue)
                    {
                        codes[i + 2] = new CodeInstruction(OpCodes.Beq, (Label)codes[i + 3].operand);
                        codes.RemoveAt(i + 3);
                    }
                    else
                    {
                        codes[i + 2] = new CodeInstruction(OpCodes.Beq_S, (Label)codes[i + 2].operand);
                    }
                }
            }
            return codes;
        }
    }
}