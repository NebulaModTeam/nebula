#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UISpraycoaterWindow))]
internal class UISpraycoaterWindow_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UISpraycoaterWindow._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    private static IEnumerable<CodeInstruction> _OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Replace: if (cargoPath.TryInsertItem(Mathf.Max(4, beltComponent.segIndex + beltComponent.segPivotOffset - 20), this.player.inhandItemId, 1, (byte)num))
        // To:      if (this.traffic.PutItemOnBelt(spraycoaterComponent.cargoBeltId, this.player.inhandItemId, (byte)num))
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var matcher = new CodeMatcher(codeInstructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CargoPath), nameof(CargoPath.TryInsertItem))))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    HarmonyLib.Transpilers.EmitDelegate<Func<byte, UISpraycoaterWindow, bool>>((foo, window) =>
                    {
                        // Recalculate itemInc here because the argument is not reliable
                        var itemInc = window.player.inhandItemInc > 0
                            ? window.player.inhandItemInc / window.player.inhandItemCount
                            : 0;
                        itemInc = itemInc > 10 ? 10 : itemInc;
                        var itemId = window.player.inhandItemId;
                        var cargoBeltId = window.traffic.spraycoaterPool[window.spraycoaterId].cargoBeltId;
                        return window.traffic.PutItemOnBelt(cargoBeltId, itemId, (byte)itemInc);
                    }))
                .RemoveInstruction()
                .Advance(-17)
                .RemoveInstructions(14); // remove #81~94, leave only (byte)num
            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("UISpraycoaterWindow._OnUpdate_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }
}
