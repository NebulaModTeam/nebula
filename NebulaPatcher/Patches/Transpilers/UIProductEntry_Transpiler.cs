#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIProductEntry))]
public static class UIProductEntry_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIProductEntry.UpdateExtraProductTexts))]
    private static IEnumerable<CodeInstruction> UpdateExtraProductTexts_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Change: this.gameData.factoryCount
        //To:     GetFactoryCount()
        //Change: planetData.factoryIndex
        //To:     GetFactoryIndex(planetData)
        //Change: planetData.factory != null
        //To:     HasFactory(planetData)
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            instructions = ReplaceFactoryCount(codeInstructions);
            instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.factoryIndex)))
                )
                .Repeat(matcher => matcher
                    .SetAndAdvance(OpCodes.Call,
                        AccessTools.Method(typeof(UIProductEntry_Transpiler), nameof(GetFactoryIndex)))
                )
                .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(PlanetData), nameof(PlanetData.factory))),
                    new CodeMatch(OpCodes.Brfalse)
                )
                .Repeat(matcher => matcher
                    .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UIProductEntry_Transpiler), nameof(HasFactory)))
                )
                .InstructionEnumeration();

            return instructions;
        }
        catch
        {
            Log.Error("Transpiler UIProductEntry.UpdateExtraProductTexts failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }

    private static IEnumerable<CodeInstruction> ReplaceFactoryCount(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(UIProductEntry), nameof(UIProductEntry.gameData))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameData), nameof(GameData.factoryCount)))
            )
            .Repeat(matcher => matcher
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(UIProductEntry_Transpiler), nameof(GetFactoryCount)))
                .RemoveInstructions(2)
            )
            .InstructionEnumeration();
    }

    private static int GetFactoryCount()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return GameMain.data.factoryCount;
        }
        return Multiplayer.Session.Statistics.FactoryCount;
    }

    private static PlanetData GetPlanetData(int factoryId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return GameMain.data.factories[factoryId].planet;
        }
        return Multiplayer.Session.Statistics.GetPlanetData(factoryId);
    }

    private static int GetFactoryIndex(PlanetData planet)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return planet.factoryIndex;
        }
        return Multiplayer.Session.Statistics.GetFactoryIndex(planet);
    }

    private static bool HasFactory(PlanetData planet)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return planet.factory != null;
        }
        return Multiplayer.Session.Statistics.HasFactory(planet);
    }
}
