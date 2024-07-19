using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using System.Reflection.Emit;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.DFRelay;
using NebulaWorld;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(DFRelayComponent))]
    internal class EnemyDFRelayComponent_Transplier
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DFRelayComponent.RelaySailLogic))]
        public static IEnumerable<CodeInstruction> RelaySailLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                // Attempt to match anywhere where `direction = -1` is set.
                // Change to:
                // direction = -1
                // call UpdateRelayDirectionState(relayId, hive)

                var codeMatcher = new CodeMatcher(instructions);
                codeMatcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "direction"));

                if (codeMatcher.IsInvalid)
                {
                    Log.Error("Transpiler DFRelayComponent.RelaySailLogic matcher is not valid. Mod version not compatible with game version.");
                    return instructions;
                }

                codeMatcher.Repeat(matcher =>
                {
                    matcher.InsertAndAdvance(
                        // Relay ID argument
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,
                            AccessTools.Field(typeof(DFRelayComponent), nameof(DFRelayComponent.id))),

                        // Relay sail stage argument
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,
                            AccessTools.Field(typeof(DFRelayComponent), nameof(DFRelayComponent.stage))),

                        // Hive Astro ID argument
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,
                            AccessTools.Field(typeof(DFRelayComponent), nameof(DFRelayComponent.hiveAstroId))),

                        //Call our method
                        new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(EnemyDFRelayComponent_Transplier),
                                nameof(ReplicateRelayDirectionChange))));
                });

                return codeMatcher.InstructionEnumeration();
            }
            catch (Exception e)
            {
                Log.Error("Transpiler DFRelayComponent.RelaySailLogic failed. Mod version not compatible with game version.");
                Log.Error(e);
                return instructions;
            }
        }

        static void ReplicateRelayDirectionChange(int relayId, int stage, int hiveAstroId)
        {
            if (!Multiplayer.IsActive) return;
            if (!Multiplayer.Session.IsClient)
            {
                Multiplayer.Session.Network.SendPacket(new DFRelayDirectionStateChangePacket(relayId, hiveAstroId, stage, -1));
            }
        }
    }
}
