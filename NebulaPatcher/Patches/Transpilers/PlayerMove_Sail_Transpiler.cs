using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NebulaModel.Packets.Players;
using System.Reflection;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PlayerMove_Sail))]
    class PlayerMove_Sail_Transpiler
    {

        [HarmonyTranspiler]
        [HarmonyPatch("GameTick")]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // c# 33 c# 69 c# 79 c# 87 this.player.warpCommand = false;
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "warpCommand")
                );

            if(codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlayerMoveSail_Transpiler.GameTick failed. Mod version not compatible with game version.");
                return instructions;
            }

            instructions = codeMatcher
                .Repeat(matcher =>
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action>(() =>
                        {
                            // send to host / clients
                            if (!SimulatedWorld.Initialized)
                            {
                                return;
                            }

                            if (LocalPlayer.IsMasterClient)
                            {
                                PlayerUseWarper packet = new PlayerUseWarper(false)
                                {
                                    PlayerId = LocalPlayer.PlayerId
                                };
                                LocalPlayer.SendPacket(packet);
                            }
                            else
                            {
                                LocalPlayer.SendPacket(new PlayerUseWarper(false));
                            }

                            return;
                        }));
                })
                .InstructionEnumeration();

            // c# 42 this.player.warpCommand = true;
            codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "warpCommand")
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlayerMoveSail_Transpiler.GameTick 2 failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                .Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action>(() =>
                {
                    // send to host / clients
                    if (!SimulatedWorld.Initialized)
                    {
                        return;
                    }

                    if (LocalPlayer.IsMasterClient)
                    {
                        PlayerUseWarper packet = new PlayerUseWarper(true)
                        {
                            PlayerId = LocalPlayer.PlayerId
                        };
                        LocalPlayer.SendPacket(packet);
                    }
                    else
                    {
                        LocalPlayer.SendPacket(new PlayerUseWarper(true));
                    }

                    return;
                }))
                .InstructionEnumeration();
        }

    }
}