#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch]
internal class BuildTool_Click_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
    private static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator iL)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions, iL)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldsfld && ((FieldInfo)i.operand).Name == "buildTargetAutoMove")
            );

        if (codeMatcher.IsInvalid)
        {
            Log.Error("BuildTool_Click.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        var label = codeMatcher.InstructionAt(1).operand;
        return codeMatcher
            .Advance(2)
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value &&
                Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label))
            .InstructionEnumeration();
    }
}
