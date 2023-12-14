#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(EjectorComponent))]
internal class EjectorComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EjectorComponent.InternalUpdate))]
    private static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Store projectile data after swarm.AddBullet(sailBullet, orbitId) if IsUpdateNeeded == true
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var matcher = new CodeMatcher(codeInstructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(SailBullet), nameof(SailBullet.lBegin))) //IL#638
                );
            var loadInstruction = matcher.InstructionAt(-1);
            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt,
                        AccessTools.Method(typeof(DysonSwarm), nameof(DysonSwarm.AddBullet))) //IL#679
                )
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(EjectorComponent), nameof(EjectorComponent.planetId))),
                    loadInstruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(EjectorComponent), nameof(EjectorComponent.orbitId))),
                    HarmonyLib.Transpilers.EmitDelegate<Action<int, Vector3, int>>((planetId, localPos, orbitId) =>
                    {
                        // If the dyson sphere has no subscribers anymore, skip this data
                        if (!Multiplayer.IsActive || !Multiplayer.Session.Launch.Snapshots.ContainsKey(planetId / 100 - 1))
                        {
                            return;
                        }

                        // Assume orbitId < 65536
                        var data = new DysonLaunchData.Projectile
                        {
                            PlanetId = planetId,
                            TargetId = (ushort)orbitId,
                            LocalPos = localPos
                        };
                        Multiplayer.Session.Launch.ProjectileBag.Add(data);
                    })
                );
            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("EjectorComponent.InternalUpdate_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }
}
