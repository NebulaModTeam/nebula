#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UniverseGen))]
internal class UniverseGen_Patch
{
    // overwrite generated birth planet and star with values selected by player in the lobby
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UniverseGen.CreateGalaxy))]
    public static void CreateGalaxy_Postfix(GalaxyData __result)
    {
        if (!Multiplayer.IsActive || UIVirtualStarmap_Transpiler.CustomBirthStar == -1)
        {
            return;
        }
        Log.Debug("Overwriting with " + __result.PlanetById(UIVirtualStarmap_Transpiler.CustomBirthPlanet) + " and " +
                  __result.StarById(UIVirtualStarmap_Transpiler.CustomBirthStar));
        __result.birthPlanetId = UIVirtualStarmap_Transpiler.CustomBirthPlanet;
        __result.birthStarId = UIVirtualStarmap_Transpiler.CustomBirthStar;
    }
}
