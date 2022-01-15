using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PowerSystem))]
    internal class PowerSystem_Transpiler
    {
        public delegate void GetNum35(PowerSystem pSys, int netIndex, long num35);

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PowerSystem.RequestDysonSpherePower))]
        public static IEnumerable<CodeInstruction> PowerSystem_RequestDysonSpherePower_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Prevent dysonSphere.energyReqCurrentTick from changing on the client side
            //Change: if (this.dysonSphere != null)
            //To:     if (this.dysonSphere != null && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
            try
            {
                CodeMatcher codeMatcher = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "dysonSphere")),
                        new CodeMatch(OpCodes.Brfalse) //IL #93
                    );
                object label = codeMatcher.Instruction.operand;
                codeMatcher.Advance(1)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
                return codeMatcher.InstructionEnumeration();
            }
            catch
            {
                NebulaModel.Logger.Log.Error("PowerSystem.RequestDysonSpherePower_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
        }

        /*
         * we need num35 from the GameTick method to know if num46 stays at 0 or gets set to something.
         * as num46 is very important to compute the power state (used for the animations) client side we need to cache num35 to serve it when requested by PowerSystemUpdateRequest
         * num46 is generateCurrentTick
         * JUST TRANSFER num46 asshead
         */
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNetwork), "energyCapacity")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNetwork), "energyRequired")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add));

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PowerSystem.GameTick_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            matcher
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 48))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<GetNum35>((PowerSystem pSys, int netIndex, long num35) =>
                {
                    if (PowerSystemManager.PowerSystemAnimationCache.TryGetValue(pSys.planet.id, out var list))
                    {
                        // netIndex starts at 1
                        if(list.Count > 0 && netIndex - 1 < list.Count)
                        {
                            list[netIndex - 1] = num35;
                        }
                        else
                        {
                            list.Add(num35);
                        }
                    }
                    else
                    {
                        List<long> newList = new List<long>();
                        newList.Add(num35);

                        PowerSystemManager.PowerSystemAnimationCache.TryAdd(pSys.planet.id, newList);
                    }
                }));

            return matcher.InstructionEnumeration();
        }
    }
}
