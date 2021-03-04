using HarmonyLib;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
	// TODO: remove this when done with testing
	[HarmonyPatch(typeof(GameHistoryData), "EnqueueTech")]
	class patch
    {
		public static void Postfix(GameHistoryData __instance, int techId)
        {
			__instance.UnlockTech(techId);
        }
    }
	[HarmonyPatch(typeof(PlayerAction_Mine), "GameTick")]
	class PlayerAction_Mine_Transpiler
	{
		private static int countminingId = 0, countminingId2 = 0;
		private static int countRemoveVege = 0, countMineVein = 0;

		private static int miningId = -1;
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// search for the place where miningId is checked against 0 and then miningType is checked against Vegetable/Vein
			// as thats where we grab the miningId
			foreach(CodeInstruction instruction in instructions)
			{
				patch_findMiningId(instruction);
				patch_findMiningId2(instruction);
				patch_findRemoveVege(instruction);
				patch_findMineVein(instruction);

				// insert delegate before GetVegeData() to get miningId
				if (countminingId == 10 && ((MethodInfo)instruction.operand).Name == "GetVegeData")
				{
					yield return Transpilers.EmitDelegate<Func<int, int>>(miningId =>
					{
						if(PlayerAction_Mine_Transpiler.miningId == -1)
                        {
							PlayerAction_Mine_Transpiler.miningId = miningId;
						}
						return miningId;
					});
				}

				// insert delegate before GetVeinData() to get miningId
				if(countminingId2 == 8 && ((MethodInfo)instruction.operand).Name == "GetVeinData")
                {
					yield return Transpilers.EmitDelegate<Func<int, int>>(miningId =>
					{
						if(PlayerAction_Mine_Transpiler.miningId == -1)
                        {
							PlayerAction_Mine_Transpiler.miningId = miningId;
						}
						return miningId;
					});
                }

				// insert delegate before RemoveVegeWithComponents to send information to other clients
				if(countRemoveVege == 6 && ((MethodInfo)instruction.operand).Name == "RemoveVegeWithComponents")
                {
					yield return Transpilers.EmitDelegate<Func<int, int>>(VegeId =>
					{
						if(PlayerAction_Mine_Transpiler.miningId != -1)
                        {
							MultiplayerSession.instance.Client.OnVegetationMined(PlayerAction_Mine_Transpiler.miningId, true, GameMain.localPlanet.id);
							PlayerAction_Mine_Transpiler.miningId = -1;
						}
						return VegeId;
					});
                }

				// insert delegate after 'this.miningTick -= veinProto.MiningTime * 10000;' to send information to other clients
				if(countMineVein == 6)
                {
					yield return Transpilers.EmitDelegate<Func<int, int>>(miningTick =>
					{
						if (PlayerAction_Mine_Transpiler.miningId != -1)
						{
							MultiplayerSession.instance.Client.OnVegetationMined(PlayerAction_Mine_Transpiler.miningId, false, GameMain.localPlanet.id);
							PlayerAction_Mine_Transpiler.miningId = -1;
						}
						return miningTick;
					});
                }

				yield return instruction;
			}
		}

		private static void patch_findMineVein(CodeInstruction instruction)
        {

			if (countMineVein == 0 && instruction.opcode == OpCodes.Ldfld)
			{
				countMineVein++;
			}
			else if (countMineVein == 1 && instruction.opcode == OpCodes.Ldloc_S)
			{
				countMineVein++;
			}
			else if (countMineVein == 2 && instruction.opcode == OpCodes.Ldfld)
			{
				countMineVein++;
			}
			else if (countMineVein == 3 && instruction.opcode == OpCodes.Ldc_I4)
            {
				countMineVein++;
            }
			else if(countMineVein == 4 && instruction.opcode == OpCodes.Mul)
            {
				countMineVein++;
            }
			else if(countMineVein == 5 && instruction.opcode == OpCodes.Sub)
            {
				countMineVein++;
            }
            else
            {
				countMineVein = 0;
            }

        }

		private static void patch_findRemoveVege(CodeInstruction instruction)
        {

			if(countRemoveVege == 0 && instruction.opcode == OpCodes.Call && ((MethodInfo)instruction.operand).Name == "Create")
            {
				countRemoveVege++;
            }
			else if(countRemoveVege == 1 && instruction.opcode == OpCodes.Pop)
            {
				countRemoveVege++;
            }
			else if(countRemoveVege == 2 && instruction.opcode == OpCodes.Ldloc_1)
            {
				countRemoveVege++;
            }
			else if(countRemoveVege == 3 && instruction.opcode == OpCodes.Ldloca_S)
            {
				countRemoveVege++;
            }
			else if(countRemoveVege == 4 && instruction.opcode == OpCodes.Ldfld)
            {
				countRemoveVege++;
            }
			else if(countRemoveVege == 5 && instruction.opcode == OpCodes.Callvirt)
            {
				countRemoveVege++;
            }
            else
            {
				countRemoveVege = 0;
            }

        }

		private static void patch_findMiningId2(CodeInstruction instruction)
        {

			if(countminingId2 == 0 && instruction.opcode == OpCodes.Ldarg_0)
            {
				countminingId2++;
            }
			else if(countminingId2 == 1 && instruction.opcode == OpCodes.Ldfld)
            {
				countminingId2++;
            }
			else if(countminingId2 == 2 && instruction.opcode == OpCodes.Ldc_I4_2)
            {
				countminingId2++;
            }
			else if(countminingId2 == 3 && instruction.opcode == OpCodes.Bne_Un)
            {
				countminingId2++;
            }
			else if(countminingId2 == 4 && instruction.opcode == OpCodes.Ldloc_1)
            {
				countminingId2++;
            }
			else if(countminingId2 == 5 && instruction.opcode == OpCodes.Ldarg_0)
            {
				countminingId2++;
            }
			else if(countminingId2 == 6 && instruction.opcode == OpCodes.Ldfld)
            {
				countminingId2++;
            }
			else if(countminingId2 == 7 && instruction.opcode == OpCodes.Callvirt)
            {
				countminingId2++;
            }
            else
            {
				countminingId2 = 0;
            }

        }

		private static void patch_findMiningId(CodeInstruction instruction)
        {

			if (countminingId == 0 && instruction.opcode == OpCodes.Ldfld)
			{
				countminingId++;
			}
			else if (countminingId == 1 && instruction.opcode == OpCodes.Brfalse)
			{
				countminingId++;
			}
			else if (countminingId == 2 && instruction.opcode == OpCodes.Ldarg_0)
			{
				countminingId++;
			}
			else if (countminingId == 3 && instruction.opcode == OpCodes.Ldfld)
			{
				countminingId++;
			}
			else if (countminingId == 4 && instruction.opcode == OpCodes.Ldc_I4_1)
			{
				countminingId++;
			}
			else if (countminingId == 5 && instruction.opcode == OpCodes.Bne_Un)
			{
				countminingId++;
			}
			else if (countminingId == 6 && instruction.opcode == OpCodes.Ldloc_1)
			{
				countminingId++;
			}
			else if (countminingId == 7 && instruction.opcode == OpCodes.Ldarg_0)
			{
				countminingId++;
			}
			else if (countminingId == 8 && instruction.opcode == OpCodes.Ldfld)
			{
				countminingId++;
			}
			else if (countminingId == 9 && instruction.opcode == OpCodes.Callvirt)
			{
				countminingId++;
			}
			else
			{
				countminingId = 0;
			}

		}
	}
}
