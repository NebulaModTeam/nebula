#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Player;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(ConstructionSystem))]
internal class ConstructionSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ConstructionSystem.AddBuildTargetToModules))]
    public static IEnumerable<CodeInstruction> AddBuildTargetToModules_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Sync Prebuild.itemRequired changes by player, insert local method call after player.package.TakeTailItems
                Replace: if (num8 <= num) { this.player.mecha.constructionModule.InsertBuildTarget ... }
                With:    if (num8 <= num && IsClosestPlayer(ref pos)) { this.player.mecha.constructionModule.InsertBuildTarget ... }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Bgt_Un)
                );
            var sqrDist = codeMatcher.InstructionAt(-2).operand;
            var skipLabel = codeMatcher.Operand;
            codeMatcher.Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, sqrDist),
                    new CodeInstruction(OpCodes.Ldarg_2), //ref Vector3 pos
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ConstructionSystem_Transpiler), nameof(IsClosestPlayer))),
                    new CodeInstruction(OpCodes.Brfalse_S, skipLabel)
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler ConstructionSystem.AddBuildTargetToModules failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static bool IsClosestPlayer(float sqrDist, ref Vector3 pos)
    {
        if (!Multiplayer.IsActive || sqrDist < DroneManager.MinSqrDistance) return true;
        return sqrDist <= Multiplayer.Session.Drones.GetClosestRemotePlayerSqrDistance(pos);
    }
}
