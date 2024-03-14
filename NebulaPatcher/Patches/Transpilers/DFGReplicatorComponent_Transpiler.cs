#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(DFGReplicatorComponent))]
internal class DFGReplicatorComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DFGReplicatorComponent.LogicTick))]
    public static IEnumerable<CodeInstruction> LogicTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /* Let enemyFormation.AddUnit() authorize by server
            from:
                int num5 = enemyFormation.AddUnit();
            to:
                int num5 = AddUnit(enemyFormation, gbase, this.productFormId);
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddUnit"))
                .Repeat(
                    matcher => matcher.RemoveInstruction()
                        .Insert(
                            new CodeInstruction(OpCodes.Ldarg_1),
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DFGReplicatorComponent), nameof(DFGReplicatorComponent.productFormId))),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DFGReplicatorComponent_Transpiler), nameof(AddUnit)))
                        )
                    );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler DFGReplicatorComponent.LogicTick failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static int AddUnit(EnemyFormation enemyFormation, DFGBaseComponent gbase, int formId)
    {
        if (!Multiplayer.IsActive)
        {
            return enemyFormation.AddUnit();
        }
        if (Multiplayer.Session.IsClient)
        {
            return 0;
        }

        var portId = enemyFormation.AddUnit();
        if (portId > 0)
        {
            // Only broadcast if add unit success (vacancyCursor > 0)
            var packet = new DFGFormationAddUnitPacket(gbase.groundSystem.planet.id, gbase.id, formId, portId);
            Multiplayer.Session.Server.SendPacketToStar(packet, gbase.groundSystem.planet.star.id);
        }
        return 0; // Skip the following call to InitiateUnitDeferred
    }
}
