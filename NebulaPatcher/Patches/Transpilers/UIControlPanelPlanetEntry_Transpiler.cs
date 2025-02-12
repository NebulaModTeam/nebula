#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIControlPanelPlanetEntry))]
public static class UIControlPanelPlanetEntry_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIControlPanelPlanetEntry.UpdateBanner))]
    private static IEnumerable<CodeInstruction> RefreshSubEntries_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Change: this.planet.factory.gameData.mainPlayer.uPosition
        // To:     GameMain.player.uPosition
        try
        {
            var codeMacher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "planet"),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "factory"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_gameData"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_mainPlayer")
                )
                .RemoveInstructions(4)
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(GameMain), "get_mainPlayer"));

            return codeMacher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("Transpiler UIControlPanelPlanetEntry.UpdateBanner error.");
            return instructions;
        }
    }
}
