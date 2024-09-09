#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIDeathPanel))]
internal class UIDeathPanel_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIDeathPanel._OnOpen))]
    [HarmonyPatch(nameof(UIDeathPanel.UpdatePropertyGroup))]
    [HarmonyPatch(nameof(UIDeathPanel.UpdateRespawnOptionsGroup))]
    [HarmonyPatch(nameof(UIDeathPanel.CalculateRespawnOptionsGroup))]
    [HarmonyPatch(nameof(UIDeathPanel.OnRedeployButtonClick))]
    [HarmonyPatch(nameof(UIDeathPanel.OnReassembleButtonClick))]
    public static IEnumerable<CodeInstruction> ToSandboxMode_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            // Set respawn cost free in multiplayer session
            // from: GameMain.data.gameDesc.isSandboxMode
            // to:   GameMain.data.gameDesc.isSandboxMode | Multiplayer.IsActive

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameDesc), nameof(GameDesc.isSandboxMode))))
                .Repeat(
                    matcher => matcher
                        .Advance(1)
                        .Insert(
                            new CodeInstruction(OpCodes.Call,
                                AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.IsActive))),
                            new CodeInstruction(OpCodes.Or)
                        )
                );
            return codeMatcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            Log.Warn("Transpiler UIDeathPanel fail!");
            Log.Warn(e);
            return instructions;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIDeathPanel.Determine))]
    public static IEnumerable<CodeInstruction> Determine_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            // Balance: Change the delay of UIDeathPanel showing up after death from 1.5s to 5.0s
            // from: if (this.timeSinceKilledF > 1.5f && !GameMain.mainPlayer.respawning)
            // to:   if (this.timeSinceKilledF > UIOPEN_DELAY && !GameMain.mainPlayer.respawning)
            const float UIOPEN_DELAY = 5.0f;
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "timeSinceKilledF"),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ble_Un))
                .Advance(-1)
                .SetOperandAndAdvance(UIOPEN_DELAY);

            return codeMatcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            Log.Warn("Transpiler UIDeathPanel.Determine fail!");
            Log.Warn(e);
            return instructions;
        }
    }
}
