#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Foundation;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(BuildTool_BlueprintPaste))]
internal class BuildTool_BlueprintPaste_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
    private static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        /*
         * Inserts
         *  if(!Multiplayer.IsActive)
         * Before trying to take items, so that all prebuilds are assumed to require items while in MP
         */
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, il)
            .MatchForward(true,
                new CodeMatch(i => i.IsLdloc()), // count
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Ceq),
                new CodeMatch(OpCodes.Brfalse)
            );

        if (matcher.IsInvalid)
        {
            Log.Error(
                "BuildTool_BlueprintPaste.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        var jumpOperand = matcher.Instruction.operand;

        matcher = matcher
            .MatchBack(false,
                new CodeMatch(i => i.IsLdloc()),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "item"),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "ID"),
                new CodeMatch(i => i.IsStloc())
            );

        if (!matcher.IsInvalid)
        {
            return matcher
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() => Multiplayer.IsActive))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jumpOperand))
                .InstructionEnumeration();
        }
        Log.Error(
            "BuildTool_BlueprintPaste.CreatePrebuilds_Transpiler 2 failed. Mod version not compatible with game version.");
        return codeInstructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BuildTool_BlueprintPaste.DetermineReforms))]
    private static IEnumerable<CodeInstruction> DetermineReforms_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Broadcast the reform changes before ClearReformData()
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions).End()
            .MatchBack(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "ClearReformData")
            );

        if (matcher.IsInvalid)
        {
            Log.Warn("Transpiler BuildTool_BlueprintPaste.DetermineReforms failed.");
            return codeInstructions;
        }

        matcher.Insert(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildTool_BlueprintPaste_Transpiler), nameof(BroadcastReform)))
        );
        return matcher.InstructionEnumeration();
    }

    private static void BroadcastReform(BuildTool_BlueprintPaste buildTool)
    {
        if (!Multiplayer.IsActive) return;

        var reformTool = buildTool.player.controller.actionBuild.reformTool;
        var brushType = (reformTool != null) ? reformTool.brushType : 0;
        var brushColor = (reformTool != null) ? reformTool.brushColor : 0;
        Multiplayer.Session.Network.SendPacketToLocalStar(new FoundationBlueprintPastePacket(
            buildTool.planet.id, buildTool.reformGridIds, buildTool.tmp_levelChanges, brushType, brushColor));
    }
}
