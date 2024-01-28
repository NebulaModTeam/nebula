#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(DFGBaseComponent))]
internal class DFGBaseComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DFGBaseComponent.UpdateFactoryThreat))]
    public static IEnumerable<CodeInstruction> UpdateFactoryThreat_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Stop client from triggering LaunchAssault
            from:
                if (this.evolve.threat >= this.evolve.maxThreat) 
                { 
                    ... 
                }
            to:
                if (LaunchAssaultGuard(this.evolve.threat, this.evolve.maxThreat, this))
                {
                    ...
                }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "maxThreat"),
                    new CodeMatch(OpCodes.Blt));

            codeMatcher
                .Set(OpCodes.Brfalse_S, codeMatcher.Operand)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DFGBaseComponent_Transpiler), nameof(LaunchAssaultGuard)))
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler DFGBaseComponent.UpdateFactoryThreat failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static bool LaunchAssaultGuard(int threat, int maxThreat, DFGBaseComponent dFGBase)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer)
            return threat >= maxThreat;

        if (threat >= maxThreat)
        {
            // Make threat on client almost full and wait for server event
            dFGBase.evolve.threat = dFGBase.evolve.maxThreat - 1;
        }
        return false;
    }
}
