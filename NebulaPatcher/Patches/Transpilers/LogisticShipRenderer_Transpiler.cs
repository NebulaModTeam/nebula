#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Warning;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(LogisticShipRenderer))]
public class LogisticShipRenderer_Transpiler
{
    [HarmonyPatch(nameof(LogisticShipRenderer.Update))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var matcher = new CodeMatcher(instructions, il)
            // find for loop increment and exit condition
            .MatchForward(false,
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LogisticShipRenderer), "transport")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GalacticTransport), "stationCursor")),
                new CodeMatch(OpCodes.Blt))
            .Advance(-3)
            .CreateLabel(out var jmpToForCompare)
            .Start()
            // find stationPool access (which produces IndexOutOfBounds in rare cases)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LogisticShipRenderer), "transport")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GalacticTransport), "stationPool")),
                new CodeMatch(OpCodes.Ldloc_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LogisticShipRenderer), "transport")))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GalacticTransport), "stationPool")))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<IsOutOfBounds>((stationComponent, index) =>
            {
                if (index >= stationComponent.Length && Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
                {
                    WarningManager.DisplayCriticalWarning("IndexOutOfBounds in LogisticShipRenderer. Consider reconnecting!");
                }
                return index < stationComponent.Length;
            }))
            .Insert(new CodeInstruction(OpCodes.Brfalse, jmpToForCompare))
            // find start of our injected code
            .MatchBack(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LogisticShipRenderer), "transport")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GalacticTransport), "stationPool")),
                new CodeMatch(OpCodes.Ldloc_0))
            .CreateLabel(out var jmpToOverflowCheck)
            .Start()
            // exchange loop start ptr with our index checking
            .MatchForward(true,
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LogisticShipRenderer), "transport")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GalacticTransport), "stationCursor")),
                new CodeMatch(OpCodes.Blt))
            .SetOperandAndAdvance(jmpToOverflowCheck);

        return matcher.InstructionEnumeration();
    }

    private delegate bool IsOutOfBounds(StationComponent[] stationComponent, int index);
}
