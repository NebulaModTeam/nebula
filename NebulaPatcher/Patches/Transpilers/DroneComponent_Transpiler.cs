using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpilers;

internal delegate double patchEnergyChangeIfNeeded(double mechaValue, double subValue, int owner);

[HarmonyPatch(typeof(DroneComponent))]
internal class DroneComponent_Transpiler
{
    [HarmonyPatch]
    private class Get_InternalUpdate
    {
        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(DroneComponent),
                "InternalUpdate",
                [
                    typeof(CraftData).MakeByRefType(),
                    typeof(PlanetFactory),
                    typeof(Vector3).MakeByRefType(),
                    typeof(float),
                    typeof(float),
                    typeof(double).MakeByRefType(),
                    typeof(double).MakeByRefType(),
                    typeof(double),
                    typeof(double),
                    typeof(float).MakeByRefType()
                ]);
        }

        // we need to make sure drones fro other players do not drain energy from our mecha core.
        // replace each var in 'mechaEnergyChange -= var' and 'mechaEnergy -= var' with 0 if we need to.
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var matcher = new CodeMatcher(codeInstructions);

            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Ldind_R8),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub));

            if (matcher.IsInvalid)
            {
                Log.Error(
                    "DroneComponent_Transpiler.InternalUpdate_Transpiler 1 failed. Mod version not compatible with game version.");
                return codeInstructions;
            }

            matcher
                .Repeat(matcher => matcher
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DroneComponent), "owner")),
                            HarmonyLib.Transpilers.EmitDelegate<patchEnergyChangeIfNeeded>((mechaValue, subValue, owner) =>
                            {
                                if (!Multiplayer.IsActive)
                                {
                                    return mechaValue - subValue;
                                }

                                if (owner < 0)
                                {
                                    // drone does not belong to us (that would be 0, > 0 would be battlebase)
                                    return mechaValue;
                                }
                                return mechaValue - subValue;
                            }))
                        .Set(OpCodes.Nop, null) // remove original Sub
                );

            return matcher.InstructionEnumeration();
        }
    }
}
