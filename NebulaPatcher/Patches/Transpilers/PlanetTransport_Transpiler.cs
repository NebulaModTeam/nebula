#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlanetTransport))]
public class PlanetTransport_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlanetTransport.RefreshDispenserOnStoragePrebuildBuild))]
    public static IEnumerable<CodeInstruction> RefreshDispenserOnStoragePrebuildBuild_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            // factoryModel.gpuiManager is null for remote planets, so we need to use GameMain.gpuiManager which is initialized by nebula
            // replace : this.factory.planet.factoryModel.gpuiManager
            // with    : GameMain.gpuiManager
            var codeMatcher = new CodeMatcher(codeInstructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "gpuiManager")
                )
                .Repeat(matcher => matcher
                    .RemoveInstructions(4)
                    .SetAndAdvance(OpCodes.Call, typeof(GameMain).GetProperty("gpuiManager").GetGetMethod()
                    ));

            return codeMatcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            Log.Error("RefreshDispenserOnStoragePrebuildBuild_Transpiler fail!");
            Log.Error(e);
            return codeInstructions;
        }
    }
}
