using HarmonyLib;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(GameMain))]
    class GameMain_Transpiler
    {
        //Enable Pausing only when Multiplayer.CanPause is true:
        //Change:  if (!this._paused)
        //To:      if (!(this._paused && Multiplayer.CanPause))
        //Change:  if (this._fullscreenPaused && !this._fullscreenPausedUnlockOneFrame)
        //To:      if (this._fullscreenPaused && Multiplayer.CanPause && !this._fullscreenPausedUnlockOneFrame)

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(GameMain.FixedUpdate))]
        static IEnumerable<CodeInstruction> FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            var codeMatcher = new CodeMatcher(instructions, iL)
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

            codeMatcher
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Multiplayer), "get_" + nameof(Multiplayer.CanPause))),
                    new CodeInstruction(OpCodes.And)
                )
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameMain), "_fullscreenPaused"))
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("GameMain.FixedUpdate_Transpiler 2 failed. Mod version not compatible with game version.");
                return instructions;
            }

            var label = codeMatcher.InstructionAt(1).operand;

            return codeMatcher
                    .Advance(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Multiplayer), "get_" + nameof(Multiplayer.CanPause))),
                        new CodeInstruction(OpCodes.Brfalse_S, label)
                    )
                    .InstructionEnumeration();
        }
    }
}
