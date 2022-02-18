using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(SiloComponent))]
    class SiloComponent_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SiloComponent.InternalUpdate))]
        private static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Store projectile data after sphere.AddDysonRocket(dysonRocket, autoDysonNode) if IsUpdateNeeded == true
            try
            {
                CodeMatcher matcher = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(DysonSphere), nameof(DysonSphere.AddDysonRocket))) //IL#310
                    );
                CodeInstruction loadInstruction = matcher.InstructionAt(-1); //autoDysonNode
                matcher.Advance(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SiloComponent), nameof(SiloComponent.planetId))),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SiloComponent), nameof(SiloComponent.localPos))),
                        loadInstruction,
                        HarmonyLib.Transpilers.EmitDelegate<Action<int, Vector3, DysonNode>>((planetId, localPos, autoDysonNode) =>
                        {
                            // If the dyson sphere has no subscribers anymore, skip this data
                            if (!Multiplayer.IsActive || !Multiplayer.Session.Launch.Snapshots.ContainsKey(planetId/100-1))
                                return;

                            // Assume layerId < 16, nodeId < 4096
                            DysonLaunchData.Projectile data = new DysonLaunchData.Projectile
                            {
                                PlanetId = planetId,
                                TargetId = (ushort)((autoDysonNode.layerId << 12) | (autoDysonNode.id & 0x0FFF)),
                                LocalPos = localPos
                            };
                            Multiplayer.Session.Launch.ProjectileBag.Add(data);
                        })
                    );
                return matcher.InstructionEnumeration();
            }
            catch
            {
                NebulaModel.Logger.Log.Error("SiloComponent.InternalUpdate_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
        }
    }
}
