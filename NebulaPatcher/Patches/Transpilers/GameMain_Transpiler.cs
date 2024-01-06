#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(GameMain))]
internal class GameMain_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(GameMain.FixedUpdate))]
    private static IEnumerable<CodeInstruction> FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator iL)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

        //Enable Pausing only when there is no session or Multiplayer.Session.CanPause is true:
        //Change:  if (!this._paused)
        //To:      if (!(this._paused && (Multiplayer.Session == null || Multiplayer.Session.CanPause)))

        var codeMatcher = new CodeMatcher(codeInstructions, iL)
            .MatchForward(true,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Player), "ApplyGamePauseState")),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameMain), "_paused")),
                new CodeMatch(OpCodes.Brtrue)
            );

        if (codeMatcher.IsInvalid)
        {
            Log.Error("GameMain.FixedUpdate_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
        var skipLabel1 = codeMatcher.Instruction.operand;
        codeMatcher
            .CreateLabelAt(codeMatcher.Pos + 1, out var nextLabel1) // the label to execute normal gameTick flow
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Brfalse_S, nextLabel1), //_paused== false => enter loop
                new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                new CodeInstruction(OpCodes.Brfalse_S,
                    skipLabel1), //_paused== true && Multiplayer.Session == null => can pause, skip loop
                new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.DeclaredPropertyGetter(typeof(MultiplayerSession), nameof(MultiplayerSession.CanPause)))
            );

        return codeMatcher.InstructionEnumeration();
    }
}
