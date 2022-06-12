using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Universe;
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

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerMove_Sail.ThrowSolarSailLogic_Sandbox))]
        public static IEnumerable<CodeInstruction> ThrowSolarSailLogic_Sandbox_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Send DysonSailDataPacket whenever calling dysonSphere.swarm.AddSolarSail()

            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddSolarSail")
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlayerMove_Sail_Transpiler.ThrowSolarSailLogic_Sandbox failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                .SetInstruction (HarmonyLib.Transpilers.EmitDelegate<Func<DysonSwarm, DysonSail, int, long, int>>((swarm, sail, orbitId, expiryTime) =>
                {
                    if (Multiplayer.IsActive)
                    {
                        Multiplayer.Session.Network.SendPacket(new DysonSailDataPacket(swarm.dysonSphere.starData.index, ref sail, orbitId, expiryTime));
                    }
                    return swarm.AddSolarSail(sail, orbitId, expiryTime);
                }))
                .InstructionEnumeration();
        }
    }
}