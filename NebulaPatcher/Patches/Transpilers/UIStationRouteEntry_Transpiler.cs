#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIStationRouteEntry))]
public static class UIStationRouteEntry_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIStationRouteEntry.Refresh))]
    private static IEnumerable<CodeInstruction> Refresh_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Avoid calling function of null factory on client side.
        // Change: string text = this.gameData.galaxy.PlanetById(this.otherStation.planetId).factory.ReadExtraInfoOnEntity(this.otherStation.entityId);
        // To:     string text = ReadExtraInfoOnEntity(this.gameData.galaxy.PlanetById.factory, this.otherStation.entityId);
        try
        {
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "ReadExtraInfoOnEntity")
                )
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UIStationRouteEntry_Transpiler), nameof(ReadExtraInfoOnEntity)));

            return codeMatcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("Transpiler UIStationRouteEntry.Refresh error.");
            return instructions;
        }
    }

    private static string ReadExtraInfoOnEntity(PlanetFactory factory, int entityId)
    {
        return factory?.ReadExtraInfoOnEntity(entityId) ?? string.Empty;
    }
}
