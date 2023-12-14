#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlayerMove_Sail))]
internal class PlayerMove_Sail_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerMove_Sail.ThrowSolarSailLogic_Sandbox))]
    public static IEnumerable<CodeInstruction> ThrowSolarSailLogic_Sandbox_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Send DysonSailDataPacket whenever calling dysonSphere.swarm.AddSolarSail()

        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddSolarSail")
            );

        if (!codeMatcher.IsInvalid)
        {
            return codeMatcher
                .SetInstruction(HarmonyLib.Transpilers.EmitDelegate<Func<DysonSwarm, DysonSail, int, long, int>>(
                    (swarm, sail, orbitId, expiryTime) =>
                    {
                        if (Multiplayer.IsActive)
                        {
                            Multiplayer.Session.Network.SendPacket(new DysonSailDataPacket(swarm.dysonSphere.starData.index,
                                ref sail, orbitId, expiryTime));
                        }
                        return swarm.AddSolarSail(sail, orbitId, expiryTime);
                    }))
                .InstructionEnumeration();
        }
        Log.Error(
            "PlayerMove_Sail_Transpiler.ThrowSolarSailLogic_Sandbox failed. Mod version not compatible with game version.");
        return codeInstructions;

    }
}
