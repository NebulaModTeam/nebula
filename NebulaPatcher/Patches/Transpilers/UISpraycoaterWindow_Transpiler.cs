using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(UISpraycoaterWindow))]
    internal class UISpraycoaterWindow_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UISpraycoaterWindow._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        private static IEnumerable<CodeInstruction> _OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace: if (cargoPath.TryInsertItem(Mathf.Max(4, beltComponent.segIndex + beltComponent.segPivotOffset - 20), this.player.inhandItemId, 1, (byte)num))
            // To:      if (this.traffic.PutItemOnBelt(spraycoaterComponent.cargoBeltId, this.player.inhandItemId, (byte)num))
            try
            {
                CodeMatcher matcher = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CargoPath), nameof(CargoPath.TryInsertItem))))
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        HarmonyLib.Transpilers.EmitDelegate<Func<byte, UISpraycoaterWindow, bool>>((itemInc, window) =>
                        {
                            int itemId = window.player.inhandItemId;
                            int cargoBeltId = window.traffic.spraycoaterPool[window.spraycoaterId].cargoBeltId;
                            return window.traffic.PutItemOnBelt(cargoBeltId, itemId, itemInc);
                        }))
                    .RemoveInstruction()
                    .Advance(-17)
                    .RemoveInstructions(14); // remove #81~94, leave only (byte)num
                return matcher.InstructionEnumeration();
            }
            catch
            {
                NebulaModel.Logger.Log.Error("UISpraycoaterWindow._OnUpdate_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
        }
    }
}
