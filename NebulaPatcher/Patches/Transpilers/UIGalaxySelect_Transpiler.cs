#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIGalaxySelect))]
internal class UIGalaxySelect_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIGalaxySelect._OnUpdate))]
    public static IEnumerable<CodeInstruction> _OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        /*
         from:
            this.startButtonText.text = (this.uiCombat.active ? "黑雾设置页面返回" : "开始游戏").Translate();
	     
	     to:
	        this.startButtonText.text = UIVirtualStarmap_Transpiler.customBirthPlanet == -1 ? 
                (this.uiCombat.active ? "黑雾设置页面返回" : "开始游戏").Translate() : UIVirtualStarmap_Transpiler.customBirthPlanetName;
        */
        var matcher = new CodeMatcher(codeInstructions)
            .End()
            .MatchBack(false, new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "set_text"));
        if (matcher.IsInvalid)
        {
            Log.Warn("UIGalaxySelect_OnUpdate_Transpiler could not find injection point, not patching!");
            return codeInstructions;
        }

        matcher.Insert(
            HarmonyLib.Transpilers.EmitDelegate<Func<string, string>>((originalBtnName) =>
            {
                if (UIVirtualStarmap_Transpiler.CustomBirthPlanet == -1)
                    return originalBtnName;
                return "开始游戏".Translate() + '(' + UIVirtualStarmap_Transpiler.CustomBirthPlanetName + ')';
            }));

        return matcher.InstructionEnumeration();
    }
}
