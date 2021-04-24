using HarmonyLib;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlayerAction_Mine))]
    class PlayerAction_Mine_Transpiler
    {
        private static int miningId = -1;

        [HarmonyTranspiler]
        [HarmonyPatch("GameTick")]
        static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // insert delegate before GetVegeData() to get miningId
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Bne_Un),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "GetVegeData"))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(miningId =>
                {
                    if (PlayerAction_Mine_Transpiler.miningId == -1 || PlayerAction_Mine_Transpiler.miningId != miningId)
                    {
                        PlayerAction_Mine_Transpiler.miningId = miningId;
                    }
                    return miningId;
                })).InstructionEnumeration();

            // insert delegate before GetVeinData() to get miningId
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Bne_Un),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "GetVeinData"))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(miningId =>
                {
                    if (PlayerAction_Mine_Transpiler.miningId == -1 || PlayerAction_Mine_Transpiler.miningId != miningId)
                    {
                        PlayerAction_Mine_Transpiler.miningId = miningId;
                    }
                    return miningId;
                })).InstructionEnumeration();

            // insert delegate before RemoveVegeWithComponents to send information to other clients
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Create"),
                    new CodeMatch(OpCodes.Pop),
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RemoveVegeWithComponents"))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(VegeId =>
                {
                    if (PlayerAction_Mine_Transpiler.miningId != -1)
                    {
                        OnVegetationMined(PlayerAction_Mine_Transpiler.miningId, true);
                        PlayerAction_Mine_Transpiler.miningId = -1;
                    }
                    return VegeId;
                })).InstructionEnumeration();

            // insert delegate after 'this.miningTick -= veinProto.MiningTime * 10000;' to send information to other clients
            instructions = new CodeMatcher(instructions)
                .MatchForward(true, // move at the end of the matches
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Sub))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(miningTick =>
                {
                    if (PlayerAction_Mine_Transpiler.miningId != -1)
                    {
                        OnVegetationMined(PlayerAction_Mine_Transpiler.miningId, false);
                        PlayerAction_Mine_Transpiler.miningId = -1;
                    }
                    return miningTick;
                })).InstructionEnumeration();

            return instructions;
        }

        private static void OnVegetationMined(int id, bool isVege)
        {
            if (!SimulatedWorld.Initialized)
            {
                return;
            }
            var packet = new VegeMined(id, isVege);
            packet.PlayerId = LocalPlayer.PlayerId;
            LocalPlayer.SendPacket(packet);
        }
    }
}
