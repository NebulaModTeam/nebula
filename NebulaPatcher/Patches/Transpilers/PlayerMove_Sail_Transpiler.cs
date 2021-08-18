using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PlayerMove_Sail))]
    class PlayerMove_Sail_Transpiler
    {

        [HarmonyTranspiler]
        [HarmonyPatch("GameTick")]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Send PlayerUseWarper(bool) whenever warpCommand is toggled between true or false
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "warpCommand")
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlayerMoveSail_Transpiler.GameTick failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                .Repeat(matcher =>
                {
                    var warpCommand = matcher.InstructionAt(-1).opcode == OpCodes.Ldc_I4_1;
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action>(() =>
                        {
                            // send to host / clients
                            if (!SimulatedWorld.Instance.Initialized)
                            {
                                return;
                            }

                            if (LocalPlayer.Instance.IsMasterClient)
                            {
                                PlayerUseWarper packet = new PlayerUseWarper(warpCommand)
                                {
                                    PlayerId = LocalPlayer.Instance.PlayerId
                                };
                                LocalPlayer.Instance.SendPacket(packet);
                            }
                            else
                            {
                                LocalPlayer.Instance.SendPacket(new PlayerUseWarper(warpCommand));
                            }

                            return;
                        }));
                })
                .InstructionEnumeration();
        }

    }
}