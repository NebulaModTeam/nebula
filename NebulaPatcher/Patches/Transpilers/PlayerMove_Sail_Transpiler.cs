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
    internal class PlayerMove_Sail_Transpiler
    {

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerMove_Sail.GameTick))]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Send PlayerUseWarper(bool) whenever warpCommand is toggled between true or false
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
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
                    bool warpCommand = matcher.InstructionAt(-1).opcode == OpCodes.Ldc_I4_1;
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action>(() =>
                        {
                            if (Multiplayer.IsActive)
                            {
                                if (Multiplayer.Session.LocalPlayer.IsHost)
                                {
                                    PlayerUseWarper packet = new PlayerUseWarper(warpCommand)
                                    {
                                        PlayerId = Multiplayer.Session.LocalPlayer.Id
                                    };
                                    Multiplayer.Session.Network.SendPacket(packet);
                                }
                                else
                                {
                                    Multiplayer.Session.Network.SendPacket(new PlayerUseWarper(warpCommand));
                                }
                            }
                        }));
                })
                .InstructionEnumeration();
        }

    }
}