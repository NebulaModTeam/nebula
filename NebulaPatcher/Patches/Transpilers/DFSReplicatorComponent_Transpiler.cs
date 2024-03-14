#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(DFSReplicatorComponent))]
internal class DFSReplicatorComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DFSReplicatorComponent.LogicTick))]
    public static IEnumerable<CodeInstruction> LogicTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /* Let enemyFormation.AddUnit() authorize by server
            from:
                int num5 = enemyFormation.AddUnit();
            to:
                int num5 = AddUnit(enemyFormation, hive, this.productFormId);
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddUnit"))
                .Repeat(
                    matcher => matcher.RemoveInstruction()
                        .Insert(
                            new CodeInstruction(OpCodes.Ldarg_1),
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DFSReplicatorComponent), nameof(DFSReplicatorComponent.productFormId))),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DFSReplicatorComponent_Transpiler), nameof(AddUnit)))
                        )
                    );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler DFSReplicatorComponent.LogicTick failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static int AddUnit(EnemyFormation enemyFormation, EnemyDFHiveSystem hive, int formId)
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
            var packet = new DFSFormationAddUnitPacket(hive.hiveAstroId, formId, portId);
            Multiplayer.Session.Server.SendPacket(packet);
        }
        return 0; // Skip the following call to InitiateUnitDeferred
    }
}
