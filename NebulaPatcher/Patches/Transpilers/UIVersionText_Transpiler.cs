#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIVersionText))]
internal class UIVersionText_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIVersionText.Refresh))]
    public static IEnumerable<CodeInstruction> Refresh_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions, iL)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "userName")
            );

        if (codeMatcher.IsInvalid)
        {
            // For XGP version
            codeMatcher.Start()
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_usernameAndSuffix")
                );
        }

        if (!codeMatcher.IsInvalid)
        {
            return codeMatcher
                .Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<string, string>>(text =>
                {
                    if (Multiplayer.IsActive)
                    {
                        text = $"{PluginInfo.PLUGIN_SHORT_NAME} {PluginInfo.PLUGIN_DISPLAY_VERSION}\r\n{text}";
                    }
                    return text;
                }))
                .InstructionEnumeration();
        }
        Log.Warn("UIVersionText.Refresh_Transpiler failed. Mod version not compatible with game version.");
        return codeInstructions;

    }
}
