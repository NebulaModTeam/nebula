#region

using System;
using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlanetATField))]
internal class PlanetATField_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetATField.TestRelayCondition))]
    public static void TestRelayCondition_Postfix(PlanetATField __instance, Vector3 relayPos, ref bool __result)
    {
        if (!Multiplayer.IsActive) return;

        // If vanilla check already rejected landing (result == false), preserve it
        if (!__result) return;

        // If shields have energy and working generators
        if (__instance.energy > 0 && __instance.generatorCount > 0)
        {
            // 1. Full globe coverage check (95%+ coverage ratio or isSpherical)
            if (__instance.globeDefenceCoveryRatio >= 0.95 ||
                __instance.globeFillRatio >= 0.95 ||
                __instance.isSpherical)
            {
                __result = false;
                return;
            }

            // 2. Point-based coverage check against active generators
            if (__instance.generatorMatrix != null && relayPos.sqrMagnitude > 0.001f)
            {
                var relayDist = relayPos.magnitude;
                var relayDir = relayPos / relayDist;
                var count = Math.Min(__instance.generatorCount, __instance.generatorMatrix.Length);

                for (var i = 0; i < count; i++)
                {
                    var gen = __instance.generatorMatrix[i];
                    var genPos = new Vector3(gen.x, gen.y, gen.z);
                    var genDist = genPos.magnitude;
                    var radius = gen.w;

                    if (genDist > 0.001f && radius > 0f)
                    {
                        var chordDist = Vector3.Distance(relayDir * genDist, genPos);
                        if (chordDist <= radius * 1.05f)
                        {
                            __result = false;
                            return;
                        }
                    }
                }
            }
        }
    }
}
