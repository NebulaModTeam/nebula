using HarmonyLib;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(GameMain))]
    internal class GameMain_Transpiler
    {
        //Enable Pausing only when there is no session or  Multiplayer.Session.CanPause is true:
        //Change:  if (!this._paused)
        //To:      if (!(this._paused && (Multiplayer.Session == null || Multiplayer.Session.CanPause)))
        //Change:  if (this._fullscreenPaused && !this._fullscreenPausedUnlockOneFrame)
        //To:      if (this._fullscreenPaused && (Multiplayer.Session == null || Multiplayer.Session.CanPause) && !this._fullscreenPausedUnlockOneFrame)

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GameMain.FixedUpdate))]
        private static IEnumerable<CodeInstruction> FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, iL)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Player), "ApplyGamePauseState")),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameMain), "_paused")),
                    new CodeMatch(OpCodes.Brtrue)
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("GameMain.FixedUpdate_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
            object skipLabel1 = codeMatcher.Instruction.operand;
            codeMatcher
                .CreateLabelAt(codeMatcher.Pos + 1, out Label nextLabel1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Brfalse_S, nextLabel1), //_paused== false => enter loop
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                    new CodeInstruction(OpCodes.Brfalse_S, skipLabel1), //_paused== true && Multiplayer.Session == null => can pause, skip loop
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(MultiplayerSession), nameof(MultiplayerSession.CanPause)))
                )
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameMain), "_fullscreenPaused")),
                    new CodeMatch(OpCodes.Brfalse)
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("GameMain.FixedUpdate_Transpiler 2 failed. Mod version not compatible with game version.");
                return instructions;
            }

            object skipLabel2 = codeMatcher.Instruction.operand;
            return codeMatcher
                    .Advance(1)
                    .CreateLabel(out Label nextLabel2) //position of checking _fullscreenPausedUnlockOneFrame
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                        new CodeInstruction(OpCodes.Brfalse_S, nextLabel2), //_fullscreenPaused && Multiplayer.Session == null => can pause, jump to next check
                        new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(MultiplayerSession), nameof(MultiplayerSession.CanPause))),
                        new CodeInstruction(OpCodes.Brfalse_S, skipLabel2) //_fullscreenPaused && Multiplayer.Session.CanPause == fasle => can't pause, skip
                    )
                    .InstructionEnumeration();
        }
    }
}
