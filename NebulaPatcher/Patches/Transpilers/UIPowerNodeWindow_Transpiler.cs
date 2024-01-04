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

[HarmonyPatch(typeof(UIPowerNodeWindow))]
internal class UIPowerNodeWindow_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIPowerNodeWindow._OnUpdate))]
    public static IEnumerable<CodeInstruction> OnUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {

            // from: if (powerNodeComponent.requiredEnergy > powerNodeComponent.idleEnergyPerTick && this.player.mecha.energyChanges[2] > 0.0)
            // to:   if (powerNodeComponent.requiredEnergy > powerNodeComponent.idleEnergyPerTick)
            var codeMatcher = new CodeMatcher(codeInstructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_mecha"),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "energyChanges")
                )
                .RemoveInstructions(8);

            // from: this.chargeStateValueText.text = "正在充电".Translate();
            // to:   this.chargeStateValueText.text = ChargeStateText("正在充电".Translate());
            codeMatcher.MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "chargeStateValueText"))
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "set_text")
                )
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UIPowerNodeWindow_Transpiler), nameof(ChargeStateText)))
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            Log.Warn("UIPowerNodeWindow._OnUpdate Transpiler failed. Charger UI will be unchanged.");
            Log.Warn(e);
            return codeInstructions;
        }
    }

    private static string ChargeStateText(string oringalValue, UIPowerNodeWindow powerNodeWindow)
    {
        if (!Multiplayer.IsActive)
        {
            return oringalValue;
        }
        var hashId = ((long)powerNodeWindow.factory.planetId << 32) | (long)powerNodeWindow.nodeId;
        Multiplayer.Session.PowerTowers.RemoteChargerHashIds.TryGetValue(hashId, out var count);
        return oringalValue + '[' + count + ']';
    }
}
