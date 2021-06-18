using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NebulaModel.Packets.Players;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PlayerMove_Sail))]
    class PlayerMove_Sail_Transpiler
    {

        [HarmonyTranspiler]
        [HarmonyPatch("GameTick")]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            // c# 33 c# 69 c# 79 c# 87 this.player.warpCommand = false;
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldstr))
                .Repeat(matcher =>
                {
                    found = true;
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0)) // just to feed the delegate function
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(dummy =>
                        {
                            // send to host / clients
                            if (!SimulatedWorld.Initialized)
                            {
                                return 0;
                            }

                            if (LocalPlayer.IsMasterClient)
                            {
                                PlayerUseWarper packet = new PlayerUseWarper(false);
                                packet.PlayerId = LocalPlayer.PlayerId;
                                LocalPlayer.SendPacket(packet);
                            }
                            else
                            {
                                LocalPlayer.SendPacket(new PlayerUseWarper(false));
                            }

                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                })
                .InstructionEnumeration();

                if(!found)
                    NebulaModel.Logger.Log.Error("PlayerMove_Sail_Transpiler GameTick failure");

            // c# 42 this.player.warpCommand = true;
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldstr))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)) // just to feed the delegate function
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>(dummy =>
                {
                    // send to host / clients
                    if (!SimulatedWorld.Initialized)
                    {
                        return 0;
                    }

                    if (LocalPlayer.IsMasterClient)
                    {
                        PlayerUseWarper packet = new PlayerUseWarper(true);
                        packet.PlayerId = LocalPlayer.PlayerId;
                        LocalPlayer.SendPacket(packet);
                    }
                    else
                    {
                        LocalPlayer.SendPacket(new PlayerUseWarper(true));
                    }

                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            return instructions;
        }

    }
}