using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(UIVersionText))]
    class UIVersionText_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch("Refresh")]
        public static IEnumerable<CodeInstruction> Refresh_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            var codeMatcher = new CodeMatcher(instructions, iL)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "userName")
                    );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("UIVersionText.Refresh_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                .Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<string, string>>((text) =>
                {
                    if (SimulatedWorld.Initialized)
                    {
                        text = $"{PluginInfo.PLUGIN_SHORT_NAME} {PluginInfo.PLUGIN_VERSION_WITH_SHORT_SHA}\r\n{text}";
                    }
                    return text;
                }))
                .InstructionEnumeration();
        }
    }
}
